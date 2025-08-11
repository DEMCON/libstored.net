// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that simulates loopback by immediately decoding a copy of the encoded buffer.
/// Does not use an intermediate buffer; the data is copied and passed directly to Decode.
/// </summary>
public class LoopbackNoBufferLayer : ProtocolLayer
{
    /// <summary>
    /// Encodes the buffer by copying it and immediately passing it to the Decode method.
    /// </summary>
    /// <param name="buffer">The buffer to encode and loop back.</param>
    /// <param name="last">Indicates if this is the last buffer in the message (ignored).</param>
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        Span<byte> copy = new byte[buffer.Length];
        buffer.CopyTo(copy); // Simulate loopback by copying the buffer
        base.Decode(copy);
    }
}
