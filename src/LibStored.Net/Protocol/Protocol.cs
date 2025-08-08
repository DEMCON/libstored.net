// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Protocol;

public class ProtocolLayer
{
    private ProtocolLayer? _up;
    private ProtocolLayer? _down;

    // ReSharper disable once EmptyConstructor
    public ProtocolLayer() { }

    public ProtocolLayer? Up() => _up;
    public ProtocolLayer? Down() => _down;

    public virtual void Decode(Span<byte> buffer) => _up?.Decode(buffer);
    public virtual void Encode(ReadOnlySpan<byte> buffer, bool last) => _down?.Encode(buffer, last);

    /// <summary>
    /// Returns the maximum amount of data to be put in one message that is encoded.
    ///
    /// If there is an MTU applicable to the physical transport (like a CAN bus),
    /// override this method to reflect that value.Layers on top will decrease the MTU
    /// when there protocol adds headers, for example.
    /// </summary>
    /// <returns>the number of bytes, or 0 for infinity</returns>
    public virtual int Mtu() => _down?.Mtu() ?? 0;

    public virtual bool Flush() => _down?.Flush() ?? false;
    public virtual void Reset() => _down?.Reset();
    public virtual void Connected() => _up?.Connected();

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