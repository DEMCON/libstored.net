// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Protocol;

/// <summary>
/// Represents a protocol layer in a protocol stack, supporting chaining and message processing.
/// Provides virtual methods for encoding, decoding, and transport-specific features.
/// </summary>
public class ProtocolLayer
{
    private ProtocolLayer? _up;
    private ProtocolLayer? _down;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolLayer"/> class.
    /// </summary>
    public ProtocolLayer() { }

    /// <summary>
    /// Gets the protocol layer above this one in the stack.
    /// </summary>
    /// <returns>The upper protocol layer, or null if this is the top layer.</returns>
    public ProtocolLayer? Up() => _up;

    /// <summary>
    /// Gets the protocol layer below this one in the stack.
    /// </summary>
    /// <returns>The lower protocol layer, or null if this is the bottom layer.</returns>
    public ProtocolLayer? Down() => _down;

    /// <summary>
    /// Decodes the provided buffer and passes it up the stack.
    /// </summary>
    /// <param name="buffer">The buffer to decode.</param>
    public virtual void Decode(Span<byte> buffer) => _up?.Decode(buffer);

    /// <summary>
    /// Encodes the provided buffer and passes it down the stack.
    /// </summary>
    /// <param name="buffer">The buffer to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
    public virtual void Encode(ReadOnlySpan<byte> buffer, bool last) => _down?.Encode(buffer, last);

    /// <summary>
    /// Returns the maximum amount of data to be put in one message that is encoded.
    /// If there is an MTU applicable to the physical transport (like a CAN bus),
    /// override this method to reflect that value. Layers on top will decrease the MTU
    /// when their protocol adds headers, for example.
    /// </summary>
    /// <returns>The number of bytes, or 0 for infinity.</returns>
    public virtual int Mtu() => _down?.Mtu() ?? 0;

    /// <summary>
    /// Flushes any buffered data in the layer.
    /// </summary>
    /// <returns>True if data was flushed; otherwise, false.</returns>
    public virtual bool Flush() => _down?.Flush() ?? false;

    /// <summary>
    /// Resets the protocol layer and any stateful data.
    /// </summary>
    public virtual void Reset() => _down?.Reset();

    /// <summary>
    /// Wraps this layer around the specified upper layer, chaining the protocol stack.
    /// </summary>
    /// <param name="up">The upper protocol layer to wrap.</param>
    public void Wrap(ProtocolLayer up)
    {
        if (up._down is not null)
        {
            throw new InvalidOperationException("Cannot wrap a layer that already has a down layer.");
        }

        up._down = this;
        _up = up;
    }

    private ProtocolLayer Bottom()
    {
        ProtocolLayer p = this;
        while (p._down is not null)
        {
            p = p._down;
        }

        return p;
    }

    private ProtocolLayer Top()
    {
        ProtocolLayer p = this;
        while (p._up is not null)
        {
            p = p._up;
        }

        return p;
    }
}
