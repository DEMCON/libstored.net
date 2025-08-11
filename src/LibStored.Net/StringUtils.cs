// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text;

namespace LibStored.Net;

/// <summary>
/// Utility methods for encoding, decoding, and formatting strings for protocol layers.
/// </summary>
public static class StringUtils
{
    /// <summary>
    /// The maximum size for stack allocation when encoding strings.
    /// </summary>
    public const int StackAllocMaxSize = 64;

    /// <summary>
    /// Encodes a string as ASCII and sends it to the protocol layer.
    /// </summary>
    /// <param name="layer">The protocol layer to send to.</param>
    /// <param name="text">The string to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
    public static void EncodeASCII(Protocol.ProtocolLayer layer, string text, bool last = false) => StringUtils.Encode(layer, text, last, Encoding.ASCII);

    /// <summary>
    /// Encodes a string as UTF-8 and sends it to the protocol layer.
    /// </summary>
    /// <param name="layer">The protocol layer to send to.</param>
    /// <param name="text">The string to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
    public static void EncodeUTF8(Protocol.ProtocolLayer layer, string text, bool last = false) => StringUtils.Encode(layer, text, last, Encoding.UTF8);

    /// <summary>
    /// Encodes a string using the specified encoding and sends it to the protocol layer.
    /// </summary>
    /// <param name="layer">The protocol layer to send to.</param>
    /// <param name="text">The string to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
    /// <param name="encoding">The encoding to use. Defaults to ASCII if null.</param>
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
    /// Trims trailing zero bytes from the buffer and decodes it to a string using the specified encoding.
    /// </summary>
    /// <param name="buffer">The buffer to decode.</param>
    /// <param name="encoding">The encoding to use. Defaults to ASCII if null.</param>
    /// <returns>The decoded string.</returns>
    public static string Decode(ReadOnlySpan<byte> buffer, Encoding? encoding = null)
    {
        encoding ??= Encoding.ASCII;

        return encoding.GetString(buffer.TrimEnd((byte)'0'));
    }

    /// <summary>
    /// Converts a string to a C# string literal, escaping special characters.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <param name="prefix">An optional prefix to prepend to the result.</param>
    /// <returns>The string literal representation.</returns>
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
}
