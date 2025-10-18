// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that segments messages into smaller chunks based on MTU and reassembles them on decode.
/// Adds end and continue markers to indicate message boundaries.
/// </summary>
public class SegmentationLayer : ProtocolLayer
{
    private const byte EndMarker = (byte)'E';
    private const byte ContinueMarker = (byte)'C';

    private readonly int _mtu;
    private List<byte> _decodeBuffer = [];

    private int _encoded;

    /// <summary>
    /// Initializes a new instance of the <see cref="SegmentationLayer"/> class.
    /// </summary>
    /// <param name="mtu">The maximum transmission unit (MTU) for segmentation. 0 means no limit.</param>
    public SegmentationLayer(int mtu = 0)
    {
        _mtu = mtu;
    }

    /// <inheritdoc />
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        ReadOnlySpan<byte> remainingBuffer = buffer;

        int mtu = LowerMtu();
        if (mtu == 0)
        {
            mtu = int.MaxValue; // Default MTU if not set
        }
        else if (mtu == 1)
        {
            mtu = 2; // Minimal value for segmentation
        }

        while (!remainingBuffer.IsEmpty)
        {
            int remaining = mtu - _encoded - 1;
            int chunk = Math.Min(remainingBuffer.Length, remaining);

            if (chunk > 0)
            {
                base.Encode(remainingBuffer.Slice(0, chunk), false);
                remainingBuffer = remainingBuffer.Slice(chunk);
            }

            if (chunk == remaining && remainingBuffer.Length > 0)
            {
                base.Encode([SegmentationLayer.ContinueMarker], true);
                _encoded = 0;
            }
            else
            {
                _encoded += chunk;
            }
        }

        if (last)
        {
            base.Encode([SegmentationLayer.EndMarker], true);
            _encoded = 0; // Reset the encoded count for the next message
        }
    }

    /// <inheritdoc />
    public override void Decode(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return; // Nothing to decode
        }

        if (_decodeBuffer.Count > 0 || buffer[^1] != SegmentationLayer.EndMarker)
        {
            if (buffer.Length > 1)
            {
                _decodeBuffer.AddRange(buffer.Slice(0, buffer.Length - 1)); // Add all but the last byte
            }

            if (buffer[^1] == SegmentationLayer.EndMarker)
            {
                Span<byte> span = CollectionsMarshal.AsSpan(_decodeBuffer);
                base.Decode(span);
                _decodeBuffer.Clear(); // Clear the buffer after decoding
            }
        }
        else
        {
            // Full package, forward to the next layer
            base.Decode(buffer.Slice(0, buffer.Length - 1));
        }
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _decodeBuffer.Clear();
        _encoded = 0;
        base.Reset();
    }

    /// <inheritdoc />
    public override int Mtu() => 0; // No MTU limit for segmentation layer

    private int LowerMtu()
    {
        int lowerMtu = base.Mtu();
        if (_mtu == 0)
        {
            return lowerMtu;
        }
        else if (lowerMtu == 0)
        {
            // Use the configured MTU if no lower MTU is set
            return _mtu;
        }
        else
        {
            return Math.Min(lowerMtu, _mtu);
        }
    }
}
