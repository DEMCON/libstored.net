// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net;

public class StoreVariable<T>
    where T : struct
{
    private readonly Store _store;

    public StoreVariable(int offset, int size, Store store)
    {
        // In C# a boolean is 4 bytes
        if (typeof(T) == typeof(bool))
        {
            if (size != 1)
            {
                throw new ArgumentException($"Size of {typeof(T).Name} is {Marshal.SizeOf<T>()}, but expected 1.");
            }
        }
        else if (Marshal.SizeOf<T>() != size)
        {
            throw new ArgumentException($"Size of {typeof(T).Name} is {Marshal.SizeOf<T>()}, but expected {size}.");
        }

        Offset = offset;
        Size = size;
        _store = store;
    }

    public int Offset { get; }
    public int Size { get; }

    public Variable<T> Variable() => _store.GetVariable<T>(Offset);

    public T Get() => Variable().Get();
    public void Set(T value) => Variable().Set(value);

    public T Value
    {
        get => Get();
        set => Set(value);
    }

    public override string ToString() => $"{Offset} with size {Size}";
}