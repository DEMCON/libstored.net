// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

public class LoopbackLayer : ProtocolLayer
{
    private readonly LoopbackSingleLayer _a2b;
    private readonly LoopbackSingleLayer _b2a;

    public LoopbackLayer(ProtocolLayer a, ProtocolLayer b)
    {
        _a2b = new LoopbackSingleLayer(a, b);
        _b2a = new LoopbackSingleLayer(b, a);
    }

    public class LoopbackSingleLayer : ProtocolLayer
    {
        private readonly ProtocolLayer _to;
        private readonly List<byte> _buffer = [];

        public LoopbackSingleLayer(ProtocolLayer from, ProtocolLayer to)
        {
            _to = to;
            Wrap(from);
        }

        /// <inheritdoc />
        public override void Encode(ReadOnlySpan<byte> buffer, bool last)
        {
            if (!buffer.IsEmpty)
            {
                _buffer.AddRange(buffer);
            }

            if (last)
            {
                Span<byte> b = CollectionsMarshal.AsSpan(_buffer);
                _to.Decode(b);
                _buffer.Clear();
            }
        }
    }
}