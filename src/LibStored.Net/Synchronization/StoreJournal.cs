// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Diagnostics;
using Id = ushort;
using Key = uint;
using Seq = ulong;

namespace LibStored.Net.Synchronization;

internal class ObjectInfo
{
    public Key Key { get; set; }
    public uint Size { get; set; }
    public Seq Seq { get; set; }

    // Not sure if we want to use this and create a binary tree in a list.
    //public Seq Highest { get; set; }
}

internal struct DecodeUpdate
{
    public Key Key { get; }
    public uint Size { get; }

    public DecodeUpdate(Key key, uint size)
    {
        Key = key;
        Size = size;
    }
}

public class StoreJournal
{
    private readonly Store _store;
    private readonly int _keySize;
    private readonly Dictionary<Key, ObjectInfo> _changes;
    private Seq _seq = 1;

    public StoreJournal(Store store)
    {
        _changes = new Dictionary<Key, ObjectInfo>(store.VariableCount);
        _store = store;
        int bufferSize = store.GetBuffer().Length;
        _keySize = (uint)bufferSize switch
        {
            <= 0xFF => 1,
            <= 0xFFFF => 2,
            <= 0xFFFFFFFF => 4,
        };
    }

    public Seq Seq => _seq;

    internal IEnumerable<KeyValuePair<Key, ObjectInfo>> Changes() => _changes;

    /// <summary>
    /// Record a change.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="size">The size may change to strings for example.</param>
    /// <param name="insertIfNew"></param>
    public void Changed(Key key, uint size, bool insertIfNew = true)
    {
        if (_changes.TryGetValue(key, out ObjectInfo? info))
        {
            info.Size = size;
            info.Seq = _seq;
        }
        else if (insertIfNew)
        {
            ObjectInfo newInfo = new()
            {
                Key = key,
                Size = size,
                Seq = _seq
            };
            _changes[key] = newInfo;
        }
    }

    public Span<byte> KeyToBuffer(Key key, uint len, ref bool ok)
    {
        if (ok && len > 0 && key + len > GetBuffer().Length)
        {
            ok = false;
        }

        return _store.GetBuffer().Slice((int)key, (int)len);
    }

    public bool HasChanged(Key key, Seq since)
    {
        if (!_changes.TryGetValue(key, out ObjectInfo? info))
        {
            return false;
        }

        return info.Seq >= since;
    }

    public void EncodeHash(Protocol.ProtocolLayer layer, bool last) => StoreJournal.EncodeHash(layer, _store.Hash, last);

    public bool HasChanged(Seq since)
    {
        if (_changes.Count == 0)
        {
            return false;
        }

        return _changes.Any(x => x.Value.Seq >= since);
    }

    /// <summary>
    /// Encode the full store's buffer.
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="last"></param>
    /// <returns></returns>
    public Seq EncodeBuffer(Protocol.ProtocolLayer layer, bool last)
    {
        try
        {
            _store.HookEntryRO(Types.Invalid, GetBuffer());

            layer.Encode(GetBuffer(), last);
        }
        finally
        {
            _store.HookExitRO(Types.Invalid, GetBuffer());
        }

        return BumpSeq();
    }

    /// <summary>
    /// Decodes to the full store's buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public Seq DecodeBuffer(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < GetBuffer().Length)
        {
            return 0;
        }

        // Don't use entryX/exitX, as we don't take exclusive ownership of the
        // data, we only update our local copy.
        buffer.CopyTo(_store.GetBuffer());

        _store.HookChanged(Types.Invalid, GetBuffer());

        Seq seq = _seq;
        foreach (KeyValuePair<Key, ObjectInfo> keyValuePair in _changes)
        {
            keyValuePair.Value.Seq = seq;
        }

        return BumpSeq();
    }

    /// <summary>
    /// Decode and apply updates from a message.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="recordAll"></param>
    /// <returns></returns>
    public Seq DecodeUpdates(ReadOnlySpan<byte> buffer, bool recordAll)
    {
        bool ok = true;

        int maxUpdates = buffer.Length / (_keySize * 2 + 1); // key + size + data (minimum of 1 byte for data)

        DecodeUpdate[] changes = ArrayPool<DecodeUpdate>.Shared.Rent(maxUpdates);
        try
        {
            int index = 0;
            while (!buffer.IsEmpty)
            {
                Key key = DecodeKey(ref buffer, ref ok);
                uint size = DecodeKey(ref buffer, ref ok);
                Span<byte> obj = KeyToBuffer(key, size, ref ok);
                if (!ok || obj.Length < size)
                {
                    return 0;
                }

                buffer.Slice(0, (int)size).CopyTo(obj);
                buffer = buffer.Slice((int)size);
                Changed(key, size, recordAll);
                changes[index++] = new DecodeUpdate(key, size);
            }

            // Don't use entryX/exitX, as we don't take exclusive ownership of the
            // data, we only update our local copy.

            for (int i = 0; i < index; i++)
            {
                DecodeUpdate change = changes[i];
                _store.HookChanged(Types.Invalid, KeyToBuffer(change.Key, change.Size, ref ok));
            }
        }
        finally
        {
            ArrayPool<DecodeUpdate>.Shared.Return(changes);
        }

        return ok ? BumpSeq() : 0;
    }

    public Seq EncodeUpdates(Span<byte> buffer, Seq since, out int bytesWritten)
    {
        bytesWritten = 0;
        foreach (KeyValuePair<Key, ObjectInfo> change in _changes
                     .Where(c => c.Value.Seq >= since)
                     .OrderBy(c => c.Value.Seq))
        {
            int size = _keySize * 2 + (int)change.Value.Size;
            EncodeUpdate(buffer.Slice(0, size), change.Value);
            buffer = buffer.Slice(size);
            bytesWritten += size;
        }

        return BumpSeq();
    }

    public int UpdateBufferSize() => GetBuffer().Length + _store.VariableCount * 8;

    private void EncodeUpdate(Span<byte> buffer, ObjectInfo info)
    {
        EncodeKey(buffer.Slice(0, _keySize), info.Key);
        EncodeKey(buffer.Slice(_keySize, _keySize), info.Size);

        bool ok = true;

        ReadOnlySpan<byte> span = KeyToBuffer(info.Key, info.Size, ref ok);
        try
        {
            _store.HookEntryRO(Types.Invalid, span);
            span.CopyTo(buffer.Slice(_keySize * 2, (int)info.Size));
        }
        finally
        {
            _store.HookExitRO(Types.Invalid, span);
        }
    }

    private void EncodeKey(Span<byte> buffer, Key key)
    {
        if (_keySize == 1)
        {
            ByteUtils.WriteUInt8(buffer, (byte)key, bigEndian: false);
        }
        else if (_keySize == 2)
        {
            ByteUtils.WriteUInt16(buffer, (ushort)key, bigEndian: false);
        }
        else if (_keySize == 4)
        {
            ByteUtils.WriteUInt32(buffer, key, bigEndian: false);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(StoreJournal._keySize), "Unsupported key size.");
        }
    }

    private Key DecodeKey(ref ReadOnlySpan<byte> buffer, ref bool ok)
    {
        if (buffer.Length < _keySize)
        {
            ok = false;
            return 0;
        }

        ReadOnlySpan<byte> keyBuffer = buffer.Slice(0, _keySize);
        uint key = _keySize switch
        {
            1 => ByteUtils.ReadUInt8(keyBuffer, bigEndian: false),
            2 => ByteUtils.ReadUInt16(keyBuffer, bigEndian: false),
            4 => ByteUtils.ReadUInt32(keyBuffer, bigEndian: false),
            _ => throw new ArgumentOutOfRangeException()
        };

        buffer = buffer.Slice(_keySize);
        return key;
    }

    private ReadOnlySpan<byte> GetBuffer() => _store.GetBuffer();

    internal static void EncodeHash(Protocol.ProtocolLayer layer, string hash, bool last)
    {
        Debug.Assert(hash.Length + 1 > sizeof(Id));
        StringUtils.Encode(layer, hash, last);
    }

    internal static string DecodeHash(ref Span<byte> buffer)
    {
        int endIndex = buffer.IndexOf((byte)'\0');

        string hash;
        if (endIndex > 0)
        {
            hash = StringUtils.Decode(buffer.Slice(0, endIndex));

            // Also skip the '0'
            buffer = buffer.Slice(endIndex + 1);
        }
        else
        {
            hash = string.Empty;
        }

        return hash;
    }

    internal Seq BumpSeq(bool force = false)
    {
        _seq++;

        // TODO: add logic for the short range, or always bump?

        return _seq;
    }
}