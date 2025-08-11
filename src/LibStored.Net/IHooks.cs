// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

/// <summary>
/// Defines hooks for monitoring or intercepting store operations.
/// Implement this interface to receive notifications for entry, exit, and change events on store data.
/// </summary>
public interface IHooks
{
    /// <summary>
    /// Called when entering a writeable context for a value.
    /// </summary>
    /// <param name="type">The type of the value being accessed.</param>
    /// <param name="buffer">The buffer containing the value data.</param>
    void EntryX(Types type, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Called when exiting a writeable context for a value.
    /// </summary>
    /// <param name="type">The type of the value being accessed.</param>
    /// <param name="buffer">The buffer containing the value data.</param>
    /// <param name="changed">Indicates whether the value was changed.</param>
    void ExitX(Types type, ReadOnlySpan<byte> buffer, bool changed);

    /// <summary>
    /// Called when entering a read-only context for a value.
    /// </summary>
    /// <param name="type">The type of the value being accessed.</param>
    /// <param name="buffer">The buffer containing the value data.</param>
    void EntryRO(Types type, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Called when exiting a read-only context for a value.
    /// </summary>
    /// <param name="type">The type of the value being accessed.</param>
    /// <param name="buffer">The buffer containing the value data.</param>
    void ExitRO(Types type, ReadOnlySpan<byte> buffer);

    /// <summary>
    /// Called when a value has changed.
    /// </summary>
    /// <param name="type">The type of the value that changed.</param>
    /// <param name="buffer">The buffer containing the new value data.</param>
    void Changed(Types type, ReadOnlySpan<byte> buffer);
}
