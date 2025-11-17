// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Debugging;

/// <summary>
/// Represents a debuggable variant of a value in a <see cref="Store"/>.
/// Provides access to the value's type, offset, size, and buffer operations.
/// </summary>
public class DebugVariant
{
    private readonly Store _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugVariant"/> class.
    /// </summary>
    /// <param name="store">The store containing the variant.</param>
    /// <param name="offset">The offset of the variant in the store.</param>
    /// <param name="size">The size of the variant in bytes.</param>
    /// <param name="type">The type of the variant.</param>
    internal DebugVariant(Store store, int offset, int size, Types type)
    {
        _store = store;
        Offset = offset;
        Size = size;
        Type = type;
    }

    /// <summary>
    /// Gets the offset of the variant in the store.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Gets the size of the variant in bytes.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the type of the variant.
    /// </summary>
    public Types Type { get; }

    /// <summary>
    /// Gets the value of the variant as a read-only span of bytes.
    /// </summary>
    /// <returns>A read-only span of bytes containing the value.</returns>
    public ReadOnlySpan<byte> Get() => Variant().Get();

    /// <summary>
    /// Copies the value of the variant to the specified destination span.
    /// </summary>
    /// <param name="destination">The destination span to copy the value to.</param>
    public void CopyTo(Span<byte> destination) => Variant().CopyTo(destination);

    /// <summary>
    /// Sets the value of the variant from the specified data span.
    /// </summary>
    /// <param name="data">The data to set as the value.</param>
    public void Set(ReadOnlySpan<byte> data) => Variant().Set(data);

    /// <summary>
    /// Gets a <see cref="Variant"/> object for this debug variant.
    /// </summary>
    /// <returns>The <see cref="Variant"/> representing this debug variant.</returns>
    internal Variant Variant() => _store.GetVariant(Type, Offset, Size);

    /// <summary>
    /// Un-safe access to the buffer. Mainly used to find the offset.
    /// </summary>
    /// <returns>A read-only span of bytes representing the underlying buffer.</returns>
    internal ReadOnlySpan<byte> Buffer() => Variant().Buffer();
}
