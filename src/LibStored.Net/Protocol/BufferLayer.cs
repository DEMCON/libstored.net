// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

public class BufferLayer : ProtocolLayer
{
    private readonly List<byte> _buffer = [];

    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        if (!buffer.IsEmpty)
        {
            _buffer.AddRange(buffer);
        }

        if (last)
        {
            Span<byte> b = CollectionsMarshal.AsSpan(_buffer);
            base.Encode(b, true);
            _buffer.Clear();
        }
    }
}