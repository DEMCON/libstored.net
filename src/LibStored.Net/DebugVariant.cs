// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

public class DebugVariant
{
    private readonly Store _store;

    public DebugVariant(Store store, int offset, int size, Types type)
    {
        _store = store;
        Offset = offset;
        Size = size;
        Type = type;
    }

    public int Offset { get; }
    public int Size { get; }
    public Types Type { get; }

    public Variant Variant() => _store.GetVariant(Type, Offset, Size);

    public ReadOnlySpan<byte> Get() => Variant().Get();
    public void CopyTo(Span<byte> destination) => Variant().CopyTo(destination);
    public void Set(ReadOnlySpan<byte> data) => Variant().Set(data);

    /// <summary>
    /// Un-safe access to the buffer. Mainly used to find the offset.
    /// </summary>
    /// <returns></returns>
    internal ReadOnlySpan<byte> Buffer() => Variant().Buffer();
}