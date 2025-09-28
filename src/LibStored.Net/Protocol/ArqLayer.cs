// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Protocol;

/// <summary>
/// Automatic Repeat Request layer
/// </summary>
public class ArqLayer : ProtocolLayer
{
    private const byte NopFlag = 0x40;
    private const byte AckFlag = 0x80;
    private const byte SeqMask = 0x3F;

    /// <inheritdoc/>
    public override bool Flush()
    {
        Encode([NopFlag], true);
        return true;
    }
}
