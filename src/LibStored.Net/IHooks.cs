// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net;

public interface IHooks
{
    void EntryX(Types type, ReadOnlySpan<byte> buffer);
    void ExitX(Types type, ReadOnlySpan<byte> buffer, bool changed);
    void EntryRO(Types type, ReadOnlySpan<byte> buffer);
    void ExitRO(Types type, ReadOnlySpan<byte> buffer);
    void Changed(Types type, ReadOnlySpan<byte> buffer);
}