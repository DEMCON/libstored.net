// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace LibStored.Net.Synchronization;

public class SynchronizableStore<TStore> : IHooks where TStore : Store
{
    private readonly TStore _store;
    private readonly StoreJournal _journal;

    public SynchronizableStore(TStore store)
    {
        _store = store;
        _journal = new StoreJournal(store);
        _store.Hooks = this;
    }

    public TStore Store() => _store;

    public StoreJournal Journal() => _journal;

    /// <inheritdoc />
    public void EntryX(Types type, ReadOnlySpan<byte> buffer) { }

    /// <inheritdoc />
    public void ExitX(Types type, ReadOnlySpan<byte> buffer, bool changed)
    {
        if (changed)
        {
            bool ok = true;
            uint key = _store.BufferToKey(buffer);
            Span<byte> b = _journal.KeyToBuffer(key, (uint)buffer.Length, ref ok);
            Debug.Assert(buffer.SequenceEqual(b));
            Debug.Assert(ok);

            _journal.Changed(key, (uint)buffer.Length);
        }
    }

    /// <inheritdoc />
    public void EntryRO(Types type, ReadOnlySpan<byte> buffer) { }

    /// <inheritdoc />
    public void ExitRO(Types type, ReadOnlySpan<byte> buffer) { }

    /// <inheritdoc />
    public void Changed(Types type, ReadOnlySpan<byte> buffer)
    {
        uint key = _store.BufferToKey(buffer);
        _store.Changed((int)key);
    }
}