// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

/// <summary>
/// Mainly for strings, but can be used for any reference type that has a fixed size in the store.
/// </summary>
/// <typeparam name="T"></typeparam>
public class StoreVariant<T>
{
    private static readonly Types Type = TypesExtensions.GetType<T>();

    private readonly Store _store;

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

    public Variant Variant() => _store.GetVariant(StoreVariant<T>.Type, Offset, Size);

    public ReadOnlySpan<byte> Get() => Variant().Get();
    public void CopyTo(Span<byte> destination) => Variant().CopyTo(destination);
    public void Set(ReadOnlySpan<byte> value) => Variant().Set(value);

    public int Offset { get; }
    public int Size { get; }
    public override string ToString() => $"{Offset} with size {Size}";
}