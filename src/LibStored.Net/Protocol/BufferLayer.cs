// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that buffers data until a complete message is ready to be sent.
/// Accumulates bytes across multiple Encode calls and flushes them when the 'last' flag is set.
/// </summary>
public class BufferLayer : ProtocolLayer
{
    private readonly List<byte> _buffer = [];

    /// <summary>
    /// Buffers the provided data and sends it when the 'last' flag is true.
    /// </summary>
    /// <param name="buffer">The data to encode and buffer.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
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

    /// <inheritdoc />
    public override void Reset()
    {
        _buffer.Clear();
        base.Reset();
    }
}
