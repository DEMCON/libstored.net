// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that simulates loopback communication between two protocol layers.
/// Provides bidirectional data transfer for testing or in-memory transport.
/// </summary>
public class LoopbackLayer : ProtocolLayer
{
    private readonly LoopbackSingleLayer _a2b;
    private readonly LoopbackSingleLayer _b2a;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopbackLayer"/> class, connecting two protocol layers.
    /// </summary>
    /// <param name="a">The first protocol layer.</param>
    /// <param name="b">The second protocol layer.</param>
    public LoopbackLayer(ProtocolLayer a, ProtocolLayer b)
    {
        _a2b = new LoopbackSingleLayer(a, b);
        _b2a = new LoopbackSingleLayer(b, a);
    }

    /// <summary>
    /// Represents a single direction of loopback communication between two protocol layers.
    /// </summary>
    public class LoopbackSingleLayer : ProtocolLayer
    {
        private readonly ProtocolLayer _to;
        private readonly List<byte> _buffer = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="LoopbackSingleLayer"/> class.
        /// </summary>
        /// <param name="from">The source protocol layer.</param>
        /// <param name="to">The destination protocol layer.</param>
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
                byte[] b = _buffer.ToArray();
                _buffer.Clear();
                _to.Decode(b);
            }
        }
    }
}
