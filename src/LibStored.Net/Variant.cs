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

    /// <summary>
    /// Initializes a new instance of the <see cref="Variant"/> struct.
    /// </summary>
    /// <param name="buffer">The buffer representing the value.</param>
    /// <param name="type">The type of the value.</param>
    /// <param name="store">The backing store for the value.</param>
    public Variant(Span<byte> buffer, Types type, Store store)
    {
        _buffer = buffer;
        _type = type;
        _store = store;
    }

    /// <summary>
    /// Gets the key (offset) of the variant in the store.
    /// </summary>
    /// <returns>The key (offset) of the variant.</returns>
    public uint Key() => _store.BufferToKey(_buffer);

    /// <summary>
    /// Gets a copy of the value as a read-only span of bytes.
    /// </summary>
    /// <returns>A read-only span of bytes containing the value.</returns>
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

    /// <summary>
    /// Copies the value of the variant to the specified destination span.
    /// </summary>
    /// <param name="buffer">The destination span to copy the value to.</param>
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

    /// <summary>
    /// Sets the value of the variant from the specified data span.
    /// </summary>
    /// <param name="value">The data to set as the value.</param>
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
