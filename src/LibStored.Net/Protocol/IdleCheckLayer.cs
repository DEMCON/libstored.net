// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Protocol;

/// <summary>
/// A layer that tracks if it sees communication through the stack.
///
/// This may be used to check of long inactivity on stalled or disconnected
/// communication channels.
/// </summary>
public class IdleCheckLayer : ProtocolLayer
{
    /// <summary>
    /// Checks if upstream was idle since the last call to <see cref="SetIdle"/>
    /// </summary>
    public bool IdleUp { get; private set; } = true;

    /// <summary>
    /// Checks if downstream was idle since the last call to <see cref="SetIdle"/>
    /// </summary>
    public bool IdleDown { get; private set; } = true;

    /// <summary>
    /// Checks if both up and down the stack was idle since the last call to <see cref="SetIdle"/>.
    /// </summary>
    public bool Idle => IdleUp && IdleDown;

    /// <summary>
    /// Resets the idle flags.
    /// </summary>
    public void SetIdle()
    {
        IdleUp = true;
        IdleDown = true;
    }

    /// <inheritdoc />
    public override void Decode(Span<byte> buffer)
    {
        IdleUp = false;
        base.Decode(buffer);
    }

    /// <inheritdoc />
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        IdleDown = false;
        base.Encode(buffer, last);
    }
}
