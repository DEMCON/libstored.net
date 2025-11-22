// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Debugging;

/// <summary>
/// Helper layer to merge responses for a macro.
/// </summary>
public class FrameMerger : Protocol.ProtocolLayer
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="protocolLayer"></param>
    public FrameMerger(Protocol.ProtocolLayer protocolLayer)
    {
        protocolLayer.Wrap(this);
    }

    /// <inheritdoc />
    public override void Encode(ReadOnlySpan<byte> buffer, bool last) => base.Encode(buffer, false);
}
