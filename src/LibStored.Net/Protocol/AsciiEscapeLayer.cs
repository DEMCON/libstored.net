// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

public class AsciiEscapeLayer : ProtocolLayer
{
    public const byte EscapeCharacter = 0x7f;

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
                    res.Add((byte)(b & 0x3f)); // Remove the escape character bit
                }

                escaped = false; // Reset the escape state
            }
            else if (b == AsciiEscapeLayer.EscapeCharacter)
            {
                escaped = true; // Set the escape state
            }
            else
            {
                res.Add(b); // Just copy the byte
            }
        }

        // The list will never resize, so we can safely convert it to a span
        Span<byte> resSpan = CollectionsMarshal.AsSpan(res);

        base.Decode(resSpan);
    }

    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        // Try to optimize the encoding by checking if we need to escape any characters
        if (!buffer.ContainsAnyExceptInRange<byte>(0x20, 0x7e))
        {
            base.Encode(buffer, last);
            return;
        }

        List<byte> res = new(buffer.Length);
        foreach (byte b in buffer)
        {
            if (b < 0x20)
            {
                res.Add(AsciiEscapeLayer.EscapeCharacter);
                res.Add((byte)(b | 0x40));
            }
            else if (b == AsciiEscapeLayer.EscapeCharacter)
            {
                res.Add(AsciiEscapeLayer.EscapeCharacter);
                res.Add(AsciiEscapeLayer.EscapeCharacter);
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
}