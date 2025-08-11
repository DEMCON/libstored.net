// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Diagnostics;

namespace LibStored.Net.Synchronization;

/// <summary>
/// Wraps a <see cref="Store"/> and its <see cref="StoreJournal"/> to provide synchronization and change tracking via hooks.
/// </summary>
/// <typeparam name="TStore">The type of the store being synchronized.</typeparam>
public class SynchronizableStore<TStore> : IHooks where TStore : Store
{
    private readonly TStore _store;
    private readonly StoreJournal _journal;

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizableStore{TStore}"/> class.
    /// </summary>
    /// <param name="store">The store to wrap and synchronize.</param>
    public SynchronizableStore(TStore store)
    {
        _store = store;
        _journal = new StoreJournal(store);
        _store.Hooks = this;
    }

    /// <summary>
    /// Gets the wrapped store instance.
    /// </summary>
    /// <returns>The wrapped <typeparamref name="TStore"/> instance.</returns>
    public TStore Store() => _store;

    /// <summary>
    /// Gets the <see cref="StoreJournal"/> used for tracking changes to the store.
    /// </summary>
    /// <returns>The <see cref="StoreJournal"/> associated with the store.</returns>
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
