// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

/// <summary>
/// Represents a variant that provides access to a buffer of bytes with support for read-only and mutable operations.
/// </summary>
/// <remarks>The <see cref="Variant"/> type is a read-only ref struct designed to manage a span of bytes in
/// conjunction with a backing store. It provides methods to retrieve, copy, and modify the underlying buffer while
/// ensuring thread safety and proper resource management through hooks into the associated <see
/// cref="Store"/>.</remarks>
public readonly ref struct Variant
{
    private readonly Span<byte> _buffer;
    private readonly Types _type;
    private readonly Store _store;

    public Variant(Span<byte> buffer, Types type, Store store)
    {
        _buffer = buffer;
        _type = type;
        _store = store;
    }

    public uint Key() => _store.BufferToKey(_buffer);

    public ReadOnlySpan<byte> Get()
    {
        Span<byte> bufferCopy;
        try
        {
            _store.HookEntryRO(_type, _buffer);

            // Create a copy, so the user doesn't have a span to the buffer which can change and is not thread-safe.
            bufferCopy = _buffer.ToArray();
        }
        finally
        {
            _store.HookExitRO(_type, _buffer);
        }

        return bufferCopy;
    }

    public void CopyTo(Span<byte> buffer)
    {
        try
        {
            _store.HookEntryRO(_type, _buffer);

            _buffer.CopyTo(buffer);
        }
        finally
        {
            _store.HookExitRO(_type, _buffer);
        }
    }

    public void Set(ReadOnlySpan<byte> value)
    {
        bool changed = false;
        try
        {
            _store.HookEntryX(_type, _buffer);

            changed = !_buffer.SequenceEqual(value);
            if (changed)
            {
                value.CopyTo(_buffer);
            }
        }
        finally
        {
            _store.HookExitX(_type, _buffer, changed);
        }
    }

    internal ReadOnlySpan<byte> Buffer() => _buffer;
}