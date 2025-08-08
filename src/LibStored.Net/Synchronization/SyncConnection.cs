// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Diagnostics;
using Id = ushort;
using Seq = ulong;

namespace LibStored.Net.Synchronization;

public class StoreInfo
{
    public Seq Seq { get; set; }
    public Id IdOut { get; set; }
    public bool Source { get; set; }
}

public class SyncConnection : Protocol.ProtocolLayer
{
    // Microsoft documentation and the .NET runtime itself often use 256 or 512 bytes as a practical threshold for stackalloc.
    private const int StackAllocThreshold = 256;

    private const byte Hello = (byte)'h';
    private const byte Welcome = (byte)'w';
    private const byte Update = (byte)'u';
    private const byte Bye = (byte)'b';

    private readonly Synchronizer _synchronizer;
    private readonly Dictionary<StoreJournal, StoreInfo> _stores = [];
    private readonly Dictionary<Id, StoreJournal> _idIn = [];

    private Id _inIdNext;

    public SyncConnection(Synchronizer synchronizer, Protocol.ProtocolLayer connection) : this(synchronizer)
    {
        connection.Wrap(this);
    }

    public SyncConnection(Synchronizer synchronizer)
    {
        _synchronizer = synchronizer;
    }

    public void Source(StoreJournal store)
    {
        if (_stores.TryGetValue(store, out StoreInfo? info))
        {
            Debug.Assert(info.Source);
            return;
        }

        StoreInfo si = new()
        {
            Source = true
        };
        _stores.Add(store, si);

        EncodeCmd(SyncConnection.Hello);
        store.EncodeHash(this, false);

        Id id = NextId();
        _idIn.Add(id, store);
        EncodeId(id, true);
    }

    public void Drop(StoreJournal store)
    {
        bool bye = false;
        (Id key, StoreJournal? value) = _idIn.FirstOrDefault(kv => object.ReferenceEquals(kv.Value, store));
        if (key != 0 && _idIn.ContainsKey(key))
        {
            bye = true;
            _idIn.Remove(key);
        }

        if (_stores.Remove(store))
        {
            bye = true;
        }

        if (!bye)
        {
            return;
        }

        EncodeCmd(SyncConnection.Bye);
        store.EncodeHash(this, true);
    }

    public Seq Process(StoreJournal store)
    {
        if (!_stores.TryGetValue(store, out StoreInfo? info))
        {
            // Unknown store
            return 0;
        }

        if (!store.HasChanged(info.Seq))
        {
            // No recent changes
            return 0;
        }

        // For perf it's better to combine all these updates into a single encode.
        // So first create a single buffer where all data will be stored before its encoded an send to the next protocol layer.
        // Allocate a buffer of MaxBufferSize where all updates should fit into.

        // 1 /*cmd*/ + 2 /*id*/ + BufferSize// + VariableCount * 8 /*offset/length*/,
        int idSize = sizeof(Id);
        int bufferSize = 1 + idSize + store.UpdateBufferSize();
        byte[]? bytes = null;
        Span<byte> buffer = bufferSize <= SyncConnection.StackAllocThreshold
            ? stackalloc byte[bufferSize]
            : (bytes = ArrayPool<byte>.Shared.Rent(bufferSize)).AsSpan(0, bufferSize);

        Seq seq;
        try
        {
            EncodeCmd(SyncConnection.Update, buffer.Slice(0, 1));
            EncodeId(info.IdOut, buffer.Slice(1, idSize));

            // Update the seq before the buffer is sent.
            seq = info.Seq = store.EncodeUpdates(buffer.Slice(1 + idSize), info.Seq, out int bytesWritten);
            Span<byte> encoded = buffer.Slice(0, 1 + idSize + bytesWritten);
            Encode(encoded, true);
        }
        finally
        {
            if (bytes is not null)
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }

        return seq;
    }

    /// <inheritdoc />
    public override void Decode(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return;
        }

        byte cmd = buffer[0];
        buffer = buffer.Slice(1);

        switch (cmd)
        {
            case SyncConnection.Hello:
            {
                // 'h' hash id
                string hash = StoreJournal.DecodeHash(ref buffer);
                Id id = DecodeId(buffer);

                StoreJournal? journal = _synchronizer.ToJournal(hash);
                if (journal is null)
                {
                    EncodeBye(hash);
                    break;
                }

                EncodeCmd(SyncConnection.Welcome);
                EncodeId(id);

                StoreInfo info = new()
                {
                    Source = false,
                    IdOut = id
                };
                _stores[journal] = info;

                id = NextId();
                _idIn[id] = journal;
                EncodeId(id);

                info.Seq = journal.EncodeBuffer(this, true);

                break;
            }
            case SyncConnection.Welcome:
            {
                // 'w' hello_id welcome_id buffer
                Id id = DecodeId(buffer);
                buffer = buffer.Slice(sizeof(Id));
                if (id == 0)
                {
                    break;
                }

                Id welcomeId = DecodeId(buffer);
                buffer = buffer.Slice(sizeof(Id));
                if (welcomeId == 0 || !_idIn.TryGetValue(id, out StoreJournal? journal))
                {
                    SendBye(id);
                    break;
                }

                Seq seq = journal.DecodeBuffer(buffer);
                if (seq == 0)
                {
                    SendBye(id);
                    break;
                }

                StoreInfo info = _stores[journal];
                info.Seq = seq;
                info.IdOut = welcomeId;
                Debug.Assert(info.Source);

                break;
            }
            case SyncConnection.Update:
            {
                // 'u' id updates
                Id id = DecodeId(buffer);
                buffer = buffer.Slice(sizeof(Id));
                if (id == 0 || !_idIn.TryGetValue(id, out StoreJournal? journal))
                {
                    SendBye(id);
                    break;
                }

                Process(journal);
                bool recordAll = _synchronizer.IsSynchronizing(journal, this);
                Seq seq = journal.DecodeUpdates(buffer, recordAll);
                if (seq == 0)
                {
                    SendBye(id);
                    break;
                }

                if (_stores.TryGetValue(journal, out StoreInfo? info))
                {
                    info.Seq = seq;
                }

                break;
            }
            case SyncConnection.Bye:
            {
                // 'b' hash
                // 'b' id
                // 'b'
                if (buffer.IsEmpty)
                {
                    DropNonSources();
                    HelloAgain();
                }
                else if (buffer.Length == sizeof(Id))
                {
                    Id id = DecodeId(buffer);
                    if (id == 0)
                    {
                        break;
                    }

                    if (!_idIn.TryGetValue(id, out StoreJournal? value))
                    {
                        break;
                    }

                    StoreInfo info = _stores[value];
                    if (info.Source && info.IdOut == 0)
                    {
                        HelloAgain(value);
                    }
                    else
                    {
                        EraseOut(id);
                    }
                }
                else
                {
                    string hash = StoreJournal.DecodeHash(ref buffer);
                    StoreJournal? journal = _synchronizer.ToJournal(hash);
                    if (journal is null)
                    {
                        break;
                    }

                    if (!_stores.TryGetValue(journal, out StoreInfo? info))
                    {
                        break;
                    }

                    if (info.Source && info.IdOut == 0)
                    {
                        HelloAgain(journal);
                    }
                    else
                    {
                        Erase(hash);
                    }
                }

                break;
            }
            default:
                break;
        }

        base.Decode(buffer);
    }

    public bool IsSynchronizing(StoreJournal journal) => _stores.ContainsKey(journal);

    public override void Reset()
    {
        EncodeCmd(SyncConnection.Bye);
        Flush();
        DropNonSources();
        base.Reset();
        HelloAgain();
    }

    /// <summary>
    /// Encode a Bye and drop all stores.
    /// </summary>
    protected void SendBye()
    {
        EncodeCmd(SyncConnection.Bye, true);
        _stores.Clear();
        _idIn.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hash"></param>
    protected void SendBye(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return;
        }

        Erase(hash);
        EncodeCmd(SyncConnection.Bye);
        StoreJournal.EncodeHash(this, hash, true);
    }

    /// <summary>
    /// Encode a Bye message with ID, and drop it from this connection.
    /// </summary>
    /// <param name="id"></param>
    protected void SendBye(Id id)
    {
        EraseId(id);
        EncodeCmd(SyncConnection.Bye);
        EncodeId(id, true);
    }

    private void EncodeCmd(byte c, bool last = false)
    {
        Span<byte> buffer = [c];
        Encode(buffer, last);
    }

    private void EncodeCmd(byte c, Span<byte> buffer) => buffer[0] = c;

    private void EncodeId(Id id, bool last = false)
    {
        Span<byte> buffer = stackalloc byte[sizeof(Id)];

        // What endianness to user? host / store / network
        EncodeId(id, buffer);
        Encode(buffer, last);
    }

    private void EncodeId(Id id, Span<byte> buffer) => ByteUtils.WriteUInt16(buffer, id, bigEndian: false);

    private Id DecodeId(ReadOnlySpan<byte> buffer)
    {
        if (buffer.Length < sizeof(Id))
        {
            return 0;
        }

        Id id = ByteUtils.ReadUInt16(buffer.Slice(0, 2), bigEndian: false);
        return id;
    }

    private Id NextId()
    {
        while (true)
        {
            Id id = _inIdNext++;

            if (id == 0)
            {
                continue;
            }

            if (_idIn.ContainsKey(id))
            {
                continue;
            }

            ;

            return id;
        }
    }

    private void EncodeBye(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return;
        }

        Erase(hash);
        EncodeCmd(SyncConnection.Bye);
        StoreJournal.EncodeHash(this, hash, true);
    }

    /// <summary>
    /// Remove the store from this connection that used the given ID to send updated to us.
    /// </summary>
    /// <param name="id"></param>
    private void EraseId(Id id)
    {
        if (!_idIn.Remove(id, out StoreJournal? value))
        {
            return;
        }

        _stores.Remove(value);
    }

    private void Erase(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return;
        }

        StoreJournal? journal = _synchronizer.ToJournal(hash);
        if (journal is null)
        {
            return;
        }

        _stores.Remove(journal);

        // Remove all entries in _idIn that reference this journal
        List<Id> keysToRemove = _idIn.Where(kv => object.ReferenceEquals(kv.Value, journal))
            .Select(kv => kv.Key)
            .ToList();
        foreach (Id key in keysToRemove)
        {
            _idIn.Remove(key);
        }
    }

    /// <summary>
    /// Erase the store from this connection that uses the given ID to send updates to the other party.
    /// </summary>
    /// <param name="id"></param>
    private void EraseOut(Id id)
    {
        // Find all stores with matching idOut and remove them
        List<StoreJournal> storesToRemove = _stores.Where(pair => pair.Value.IdOut == id)
            .Select(pair => pair.Key)
            .ToList();

        foreach (StoreJournal store in storesToRemove)
        {
            // Remove all m_idIn entries pointing to this store
            List<Id> idsToRemove = _idIn.Where(pair => pair.Value == store)
                .Select(pair => pair.Key)
                .ToList();

            foreach (Id removeId in idsToRemove)
            {
                _idIn.Remove(removeId);
            }

            _stores.Remove(store);
        }
    }

    /// <summary>
    /// Drop all stores from this connection that are not sources.
    /// </summary>
    public void DropNonSources()
    {
        // Find all stores that are not sources
        List<StoreJournal> storesToRemove = _stores.Where(pair => !pair.Value.Source)
            .Select(pair => pair.Key)
            .ToList();

        // Remove them from the dictionary
        foreach (StoreJournal store in storesToRemove)
        {
            _stores.Remove(store);
        }
    }

    /// <summary>
    /// Send a Hello again for all sources.
    /// </summary>
    private void HelloAgain()
    {
        foreach (KeyValuePair<StoreJournal, StoreInfo> keyValuePair in _stores)
        {
            HelloAgain(keyValuePair.Key);
        }
    }

    /// <summary>
    /// Send a Hello again for the given source.
    /// </summary>
    /// <param name="store"></param>
    private void HelloAgain(StoreJournal store)
    {
        if (!_stores.TryGetValue(store, out StoreInfo? info))
        {
            return;
        }

        info.IdOut = 0;

        Id id = _idIn.FirstOrDefault(x => x.Value == store).Key;

        Debug.Assert(id > 0);

        EncodeCmd(SyncConnection.Hello);
        store.EncodeHash(this, false);
        EncodeId(id, true);
    }
}