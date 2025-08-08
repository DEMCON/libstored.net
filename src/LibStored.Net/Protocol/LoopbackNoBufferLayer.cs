// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Protocol;

public class LoopbackNoBufferLayer : ProtocolLayer
{
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        Span<byte> copy = new byte[buffer.Length];
        buffer.CopyTo(copy); // Simulate loopback by copying the buffer
        base.Decode(copy);
    }
}