// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Buffers.Binary;

namespace LibStored.Net;

/// <summary>
/// Utility methods for reading and writing primitive types to byte buffers with configurable endianness.
/// </summary>
public static class ByteUtils
{
    /// <summary>
    /// Writes a 32-bit integer to the buffer in the specified endianness.
    /// </summary>
    /// <param name="buffer">The buffer to write to (must be at least 4 bytes).</param>
    /// <param name="value">The integer value to write.</param>
    /// <param name="bigEndian">If true, writes as big-endian; otherwise, little-endian.</param>
    public static void WriteInt32(Span<byte> buffer, int value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 32-bit integer from the buffer in the specified endianness.
    /// </summary>
    /// <param name="buffer">The buffer to read from (must be at least 4 bytes).</param>
    /// <param name="bigEndian">If true, reads as big-endian; otherwise, little-endian.</param>
    /// <returns>The integer value read from the buffer.</returns>
    public static int ReadInt32(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadInt32BigEndian(buffer) : BinaryPrimitives.ReadInt32LittleEndian(buffer);

    /// <summary>
    /// Writes a 16-bit integer to the buffer in the specified endianness.
    /// </summary>
    public static void WriteInt16(Span<byte> buffer, short value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteInt16BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 16-bit integer from the buffer in the specified endianness.
    /// </summary>
    public static short ReadInt16(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadInt16BigEndian(buffer) : BinaryPrimitives.ReadInt16LittleEndian(buffer);

    /// <summary>
    /// Writes a 16-bit unsigned integer to the buffer in the specified endianness.
    /// </summary>
    public static void WriteUInt16(Span<byte> buffer, ushort value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 16-bit unsigned integer from the buffer in the specified endianness.
    /// </summary>
    public static ushort ReadUInt16(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadUInt16BigEndian(buffer) : BinaryPrimitives.ReadUInt16LittleEndian(buffer);

    /// <summary>
    /// Writes a 16-bit unsigned integer to the buffer in the specified endianness.
    /// </summary>
    public static void WriteUInt8(Span<byte> buffer, byte value, bool bigEndian = false)
    {
        if (buffer.IsEmpty)
        {
            throw new ArgumentException("Buffer must be at least 1 byte long.", nameof(buffer));
        }

        buffer[0] = value;
    }

    /// <summary>
    /// Reads a 16-bit unsigned integer from the buffer in the specified endianness.
    /// </summary>
    public static byte ReadUInt8(ReadOnlySpan<byte> buffer, bool bigEndian = false)
    {
        if (buffer.IsEmpty)
        {
            throw new ArgumentException("Buffer must be at least 1 byte long.", nameof(buffer));
        }

        return buffer[0];
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer to the buffer in the specified endianness.
    /// </summary>
    public static void WriteUInt32(Span<byte> buffer, uint value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 32-bit unsigned integer from the buffer in the specified endianness.
    /// </summary>
    public static uint ReadUInt32(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(buffer) : BinaryPrimitives.ReadUInt32LittleEndian(buffer);

    /// <summary>
    /// Writes a 64-bit integer to the buffer in the specified endianness.
    /// </summary>
    public static void WriteInt64(Span<byte> buffer, long value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 64-bit integer from the buffer in the specified endianness.
    /// </summary>
    public static long ReadInt64(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadInt64BigEndian(buffer) : BinaryPrimitives.ReadInt64LittleEndian(buffer);

    /// <summary>
    /// Writes a 64-bit unsigned integer to the buffer in the specified endianness.
    /// </summary>
    public static void WriteUInt64(Span<byte> buffer, ulong value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 64-bit unsigned integer from the buffer in the specified endianness.
    /// </summary>
    public static ulong ReadUInt64(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadUInt64BigEndian(buffer) : BinaryPrimitives.ReadUInt64LittleEndian(buffer);

    /// <summary>
    /// Writes a 32-bit floating point value to the buffer in the specified endianness.
    /// </summary>
    public static void WriteFloat(Span<byte> buffer, float value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteSingleBigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 32-bit floating point value from the buffer in the specified endianness.
    /// </summary>
    public static float ReadFloat(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadSingleBigEndian(buffer) : BinaryPrimitives.ReadSingleLittleEndian(buffer);

    /// <summary>
    /// Writes a 64-bit floating point value to the buffer in the specified endianness.
    /// </summary>
    public static void WriteDouble(Span<byte> buffer, double value, bool bigEndian = false)
    {
        if (bigEndian)
        {
            BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
        }
        else
        {
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
        }
    }

    /// <summary>
    /// Reads a 64-bit floating point value from the buffer in the specified endianness.
    /// </summary>
    public static double ReadDouble(ReadOnlySpan<byte> buffer, bool bigEndian = false)
        => bigEndian ? BinaryPrimitives.ReadDoubleBigEndian(buffer) : BinaryPrimitives.ReadDoubleLittleEndian(buffer);
}
