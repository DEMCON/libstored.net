// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net;

/// <summary>
/// Represents a strongly-typed variable stored in a memory buffer, providing read and write access to the value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the value stored in the variable. Must be a value type (<see langword="struct"/>).</typeparam>
public class StoreVariable<T>
    where T : struct
{
    private readonly Store _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="StoreVariable{T}"/> class.
    /// </summary>
    /// <param name="offset">The offset of the variable in the store buffer.</param>
    /// <param name="size">The size of the variable in bytes.</param>
    /// <param name="store">The store containing the variable.</param>
    /// <exception cref="ArgumentException">Thrown if the size does not match the expected size for <typeparamref name="T"/>.</exception>
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

    /// <summary>
    /// Gets the offset of the variable in the store buffer.
    /// </summary>
    public int Offset { get; }

    /// <summary>
    /// Gets the size of the variable in bytes.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets a <see cref="Variable{T}"/> struct for accessing the value in the store buffer.
    /// </summary>
    internal Variable<T> Variable() => _store.GetVariable<T>(Offset);

    /// <summary>
    /// Gets the value of the variable from the store buffer.
    /// </summary>
    /// <returns>The value of type <typeparamref name="T"/>.</returns>
    public T Get() => Variable().Get();
    /// <summary>
    /// Sets the value of the variable in the store buffer.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void Set(T value) => Variable().Set(value);

    /// <summary>
    /// Gets or sets the value of the variable in the store buffer.
    /// </summary>
    public T Value
    {
        get => Get();
        set => Set(value);
    }

    /// <summary>
    /// Returns a string representation of the variable's offset and size.
    /// </summary>
    /// <returns>A string describing the offset and size.</returns>
    public override string ToString() => $"{Offset} with size {Size}";
}
