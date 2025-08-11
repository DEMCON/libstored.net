// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

/// <summary>
/// Represents a store-backed variant for reference types (such as strings) with a fixed size in the store.
/// Provides access to the underlying buffer and supports get, set, and copy operations.
/// </summary>
/// <typeparam name="T">The reference type for the variant.</typeparam>
public class StoreVariant<T>
{
    private static readonly Types Type = TypesExtensions.GetType<T>();
    private readonly Store _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreVariant{T}"/> class.
    /// </summary>
    /// <param name="offset">The offset of the variant in the store buffer.</param>
    /// <param name="size">The size of the variant in bytes.</param>
    /// <param name="store">The store containing the variant.</param>
    /// <exception cref="ArgumentException">Thrown if the size is negative.</exception>
    public StoreVariant(int offset, int size, Store store)
    {
        if (size < 0)
        {
            throw new ArgumentException($"Size must be non-negative, but was {size}.");
        }

        Offset = offset;
        Size = size;
        _store = store;
    }

    /// <summary>
    /// Gets a <see cref="Variant"/> struct for accessing the value in the store buffer.
    /// </summary>
    internal Variant Variant() => _store.GetVariant(StoreVariant<T>.Type, Offset, Size);

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
    /// <param name="value">The data to set as the value.</param>
    public void Set(ReadOnlySpan<byte> value) => Variant().Set(value);

    /// <summary>
    /// Gets the offset of the variant in the store buffer.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Gets the size of the variant in bytes.
    /// </summary>
    public int Size { get; }
    
    /// <summary>
    /// Returns a string representation of the variant's offset and size.
    /// </summary>
    /// <returns>A string describing the offset and size.</returns>
    public override string ToString() => $"{Offset} with size {Size}";
}
