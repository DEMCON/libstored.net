// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net;

/// <summary>
/// Represents a strongly-typed variable stored in a memory buffer, providing read and write access to the value of type
/// <typeparamref name="T"/>.
/// </summary>
/// <remarks>This struct is designed for scenarios where values of type <typeparamref name="T"/> are stored in a
/// memory buffer and need to be accessed or modified efficiently. It ensures proper handling of the memory buffer and
/// integrates with the associated <see cref="Store"/> for managing read-only and read-write operations.</remarks>
/// <typeparam name="T">The type of the value stored in the variable. Must be a value type (<see langword="struct"/>).</typeparam>
public readonly ref struct Variable<T>
    where T : struct
{
    private static readonly Types Type = TypesExtensions.GetType<T>();

    private readonly Span<byte> _buffer;
    private readonly Store _store;

    internal Variable(Span<byte> buffer, Store store)
    {
        if (typeof(T) == typeof(bool))
        {
            if (1 > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer is too small for the given type.");
            }
        }
        else if (Marshal.SizeOf<T>() > buffer.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(buffer), "Buffer is too small for the given type.");
        }

        _buffer = buffer;
        _store = store;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public uint Key() => _store.BufferToKey(_buffer);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        T ret;
        try
        {
            _store.HookEntryRO(Variable<T>.Type, _buffer);

            ret = MemoryMarshal.Read<T>(_buffer);
        }
        finally
        {
            _store.HookExitRO(Variable<T>.Type, _buffer);
        }

        return ret;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public void Set(T value)
    {
        bool changed = false;
        try
        {
            _store.HookEntryX(Variable<T>.Type, _buffer);

            T prev = MemoryMarshal.AsRef<T>(_buffer);

            changed = !EqualityComparer<T>.Default.Equals(prev, value);

            if (changed)
            {
                MemoryMarshal.Write(_buffer, in value);
            }
        }
        finally
        {
            _store.HookExitX(Variable<T>.Type, _buffer, changed);
        }
    }
}