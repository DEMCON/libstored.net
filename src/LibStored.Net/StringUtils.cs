// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text;

namespace LibStored.Net;

public static class StringUtils
{
    public const int StackAllocMaxSize = 64;

    public static void EncodeASCII(Protocol.ProtocolLayer layer, string text, bool last = false) => StringUtils.Encode(layer, text, last, Encoding.ASCII);
    public static void EncodeUTF8(Protocol.ProtocolLayer layer, string text, bool last = false) => StringUtils.Encode(layer, text, last, Encoding.UTF8);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="text"></param>
    /// <param name="last"></param>
    /// <param name="encoding">Default to <see cref="Encoding.ASCII"/></param>
    public static void Encode(Protocol.ProtocolLayer layer, string text, bool last = false, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        ReadOnlySpan<char> textSpan = text.AsSpan();

        int len = encoding.GetByteCount(textSpan) + 1;

        // Use the ArrayPool<byte>.Shared when do not stackalloc? Do check slice the  buffer  to the correct size before sending it.
        Span<byte> bytes = len <= StringUtils.StackAllocMaxSize ? stackalloc byte[len] : new byte[len];
        int encoded = encoding.GetBytes(textSpan, bytes);

        Debug.Assert(len == (encoded + 1), "Not all bytes are encoded.");

        layer.Encode(bytes, last);
    }

    /// <summary>
    /// Trim 0 bytes from the buffer before converting the bytes to a string using the <see cref="Encoding"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string Decode(ReadOnlySpan<byte> buffer, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        return encoding.GetString(buffer.TrimEnd((byte)'0'));
    }

    public static string StringLiteral(string text, string prefix = "")
    {
        StringBuilder sb = new(prefix);

        foreach (char c in text)
        {
            string cs = c switch
            {
                '\0' => "\\0",
                '\r' => "\\r",
                '\n' => "\\n",
                '\t' => "\\t",
                '\\' => @"\",
                < (char)0x20 or > (char)0x7e => $"\\x{(int)c:X2}",
                _ => c.ToString()
            };
            sb.Append(cs);
        }

        return sb.ToString();
    }

    //public static T Endian_h2s<T>(T value) =>
    //public static T Endian_h2l<T>(T value) where T : struct => BitConverter.ToL

    //public static Span<byte> Rev(Span<byte> b) => BinaryPrimitives.ReverseEndianness(b);
}