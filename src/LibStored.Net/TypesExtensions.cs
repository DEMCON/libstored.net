// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

/// <summary>
/// Provides extension methods for the <see cref="Types"/> enum to query type characteristics and perform type mapping.
/// </summary>
public static class TypesExtensions
{
    /// <summary>
    /// Determines whether the type has a fixed size.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is fixed size; otherwise, false.</returns>
    public static bool IsFixed(this Types type) => (type & Types.FlagFixed) != 0;

    /// <summary>
    /// Determines whether the type represents a function.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a function; otherwise, false.</returns>
    public static bool IsFunction(this Types type) => (type & Types.FlagFunction) != 0;

    /// <summary>
    /// Determines whether the type is an integer type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is an integer; otherwise, false.</returns>
    public static bool IsInt(this Types type) => (type & Types.FlagInt) != 0;

    /// <summary>
    /// Determines whether the type is signed.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is signed; otherwise, false.</returns>
    public static bool IsSigned(this Types type) => (type & Types.FlagSigned) != 0;

    /// <summary>
    /// Determines whether the type is a special type (undefined length).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is special; otherwise, false.</returns>
    public static bool IsSpecial(this Types type) => (type & Types.MaskFlags) == 0;

    /// <summary>
    /// Gets the size in bytes of the type, or 0 if not fixed size.
    /// </summary>
    /// <param name="type">The type to get the size of.</param>
    /// <returns>The size in bytes, or 0 if not fixed size.</returns>
    public static int Size(this Types type) => !type.IsFixed() ? 0 : (int)(type & Types.MaskSize) + 1;

    /// <summary>
    /// Gets the <see cref="Types"/> value corresponding to the specified generic type parameter.
    /// </summary>
    /// <typeparam name="T">The type to map.</typeparam>
    /// <returns>The corresponding <see cref="Types"/> value.</returns>
    /// <exception cref="ArgumentException">Thrown if the type is not supported.</exception>
    public static Types GetType<T>()
    {
        Type t = typeof(T);
        return t switch
        {
            _ when t == typeof(byte) => Types.Uint8,
            _ when t == typeof(sbyte) => Types.Int8,
            _ when t == typeof(short) => Types.Int16,
            _ when t == typeof(ushort) => Types.Uint16,
            _ when t == typeof(int) => Types.Int32,
            _ when t == typeof(uint) => Types.Uint32,
            _ when t == typeof(long) => Types.Int64,
            _ when t == typeof(ulong) => Types.Uint64,
            _ when t == typeof(float) => Types.Float,
            _ when t == typeof(double) => Types.Double,
            _ when t == typeof(bool) => Types.Bool,
            _ when t == typeof(IntPtr) && IntPtr.Size <= 4 => Types.Pointer32,
            _ when t == typeof(IntPtr) && IntPtr.Size > 4 => Types.Pointer64,
            _ when t == typeof(string) => Types.String,
            _ when t == typeof(byte[]) => Types.Blob,
            _ when t == typeof(void) => Types.Void,
            _ => throw new ArgumentException($"Unsupported type: {t.Name}"),
        };
    }
}
