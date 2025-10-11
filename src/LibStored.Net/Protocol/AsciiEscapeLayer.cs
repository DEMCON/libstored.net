// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that encodes and decodes ASCII escape sequences.
/// Escapes non-printable ASCII bytes and the escape character itself for safe transmission.
/// </summary>
public class AsciiEscapeLayer : ProtocolLayer
{
    /// <summary>
    /// The byte value used as the escape character (0x7f).
    /// </summary>
    public const byte EscapeCharacter = 0x7f;

    private const byte ZeroByte = 0x00;
    private const byte CrByte = 0x0D;
    private const byte XonByte = 0x11;
    private const byte XofByte = 0x13;
    private const byte EscByte = 0x1B;

    /// <summary>
    /// All the characters that need to be escaped.
    /// </summary>
    private static readonly SearchValues<byte> s_charactersToEscape = SearchValues.Create(EscapeCharacter, ZeroByte, CrByte, XonByte, XofByte, EscByte);

    /// <summary>
    /// Decodes a buffer containing ASCII escape sequences, removing escapes and restoring original bytes.
    /// </summary>
    /// <param name="buffer">The buffer to decode.</param>
    public override void Decode(Span<byte> buffer)
    {
        // Example decoding logic for ASCII escape sequences
        // TODO: Try to do modify the buffer in place, since we can only remove bytes, not add them
        // or optimize using the ArrayPool to avoid allocations (ArrayPool<byte>.Shared).

        List<byte> res = new(buffer.Length);

        bool escaped = false;
        foreach (byte b in buffer)
        {
            if (escaped)
            {
                if (b == AsciiEscapeLayer.EscapeCharacter)
                {
                    res.Add(AsciiEscapeLayer.EscapeCharacter);
                }
                else
                {
                    // Remove the escape character bit
                    res.Add((byte)(b & 0x3f));
                }

                escaped = false; // Reset the escape state
            }
            else if (b == CrByte)
            {
                // Skip the carriage return character when it is not escaped
                continue;
            }
            else if (b == AsciiEscapeLayer.EscapeCharacter)
            {
                // Set the escape state
                escaped = true;
            }
            else
            {
                // Just copy the byte
                res.Add(b);
            }
        }

        if (escaped)
        {
            // Trailing escape character, just add it
            res.Add(AsciiEscapeLayer.EscapeCharacter);
        }

        // The list will never resize, so we can safely convert it to a span
        Span<byte> resSpan = CollectionsMarshal.AsSpan(res);

        base.Decode(resSpan);
    }


    /// <summary>
    /// Encodes a buffer, escaping non-printable ASCII bytes and the escape character.
    /// </summary>
    /// <param name="buffer">The buffer to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        // Try to optimize the encoding by checking if we need to escape any characters
        if (!buffer.ContainsAny(s_charactersToEscape))
        {
            base.Encode(buffer, last);
            return;
        }

        List<byte> res = new(buffer.Length);
        foreach (byte b in buffer)
        {
            if (AsciiEscapeLayer.NeedsEscape(b))
            {
                res.Add(AsciiEscapeLayer.EscapeCharacter);
                if (b == EscapeCharacter)
                {
                    res.Add(AsciiEscapeLayer.EscapeCharacter);
                }
                else
                {
                    res.Add((byte)(b | 0x40));
                }
            }
            else
            {
                res.Add(b); // Just copy the byte
            }
        }

        // The list will never resize, so we can safely convert it to a span
        Span<byte> resSpan = CollectionsMarshal.AsSpan(res);

        base.Encode(resSpan, last);
    }

    /// <inheritdoc />
    public override int Mtu() => base.Mtu() switch
    {
        0 => 0, // No limit
        1 => 1,
        var x => x / 2,
    };

    private static bool NeedsEscape(byte b) =>
        b switch
        {
            EscapeCharacter => true,
            ZeroByte => true,
            CrByte => true,
            XonByte => true,
            XofByte => true,
            EscByte => true,
            _ => false,
        };
}
