// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Text;

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
    /// Determines whether the type is a floating point number.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is floating point number; otherwise, false.</returns>
    public static bool IsFloat(this Types type) => type.IsFixed() && type.IsSigned() && !type.IsInt();

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

    /// <summary>
    /// Parses a text as span to a Types value.
    /// The text input should have a length of 2.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Types Parse(ReadOnlySpan<char> s)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(2, s.Length, nameof(s));

        byte[] bytes = Convert.FromHexString(s);
        Types t = (Types)bytes[0];
        return t;
    }

    /// <summary>
    /// Read a value fron the buffer of the given type and size.
    /// Thrown an <see cref="ArgumentOutOfRangeException"/> when the buffer size is not the correct size for the types.
    /// Thrown an <see cref="ArgumentOutOfRangeException"/> for types: pointer/pointer32/pointer64, int, uint, void, invalid
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="type"></param>
    /// <param name="size"></param>
    /// <param name="bigEndian"></param>
    /// <returns></returns>
    public static object ReadValue(ReadOnlySpan<byte> buffer, Types type, int size, bool bigEndian = false)
    {
        // Check size
        ArgumentOutOfRangeException.ThrowIfNotEqual(type.IsFixed() ? type.Size() : size, buffer.Length, nameof(buffer));

        Types t = type & ~Types.FlagFunction;
        return t switch
        {
            Types.Bool => BitConverter.ToBoolean(buffer),
            Types.Int8 => ByteUtils.ReadInt8(buffer),
            Types.Uint8 => ByteUtils.ReadUInt8(buffer),
            Types.Int16 => ByteUtils.ReadInt16(buffer, bigEndian),
            Types.Uint16 => ByteUtils.ReadUInt16(buffer, bigEndian),
            Types.Int32 => ByteUtils.ReadInt32(buffer, bigEndian),
            Types.Uint32 or Types.Pointer32 => ByteUtils.ReadUInt32(buffer, bigEndian),
            Types.Int64 => ByteUtils.ReadInt64(buffer, bigEndian),
            Types.Uint64 or Types.Pointer64 => ByteUtils.ReadUInt64(buffer, bigEndian),
            Types.Float => ByteUtils.ReadFloat(buffer, bigEndian),
            Types.Double => ByteUtils.ReadDouble(buffer, bigEndian),
            Types.Blob => buffer.ToArray(),
            Types.String => StringUtils.Decode(buffer, Encoding.UTF8),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="buffer"></param>
    /// <param name="type"></param>
    /// <param name="size"></param>
    /// <param name="bigEndian"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void WriteValue(object value, Span<byte> buffer, Types type, int size, bool bigEndian = false)
    {
        // Check size
        ArgumentOutOfRangeException.ThrowIfNotEqual(type.IsFixed() ? type.Size() : size, buffer.Length, nameof(buffer));

        Types t = type & ~Types.FlagFunction;
        switch (t)
        {
            case Types.Bool when value is bool b:
                ByteUtils.WriteUInt8(buffer, (byte)(b ? 1 : 0), bigEndian);
                break;
            case Types.Int8 when value is sbyte v:
                ByteUtils.WriteInt8(buffer, v, bigEndian);
                break;
            case Types.Uint8 when value is byte v:
                ByteUtils.WriteUInt8(buffer, v, bigEndian);
                break;
            case Types.Int16 when value is short v:
                ByteUtils.WriteInt16(buffer, v, bigEndian);
                break;
            case Types.Uint16 when value is ushort v:
                ByteUtils.WriteUInt16(buffer, v, bigEndian);
                break;
            case Types.Int32 when value is int v:
                ByteUtils.WriteInt32(buffer, v, bigEndian);
                break;
            case Types.Uint32 when value is uint v:
                ByteUtils.WriteUInt32(buffer, v, bigEndian);
                break;
            case Types.Int64 when value is long v:
                ByteUtils.WriteInt64(buffer, v, bigEndian);
                break;
            case Types.Uint64 when value is ulong v:
                ByteUtils.WriteUInt64(buffer, v, bigEndian);
                break;
            case Types.Float when value is float v:
                ByteUtils.WriteFloat(buffer, v, bigEndian);
                break;
            case Types.Double when value is double v:
                ByteUtils.WriteDouble(buffer, v, bigEndian);
                break;
            case Types.Blob when value is byte[] v:
                v.CopyTo(buffer);
                break;
            case Types.String when value is string v:
                Encoding.UTF8.GetBytes(v, buffer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}
