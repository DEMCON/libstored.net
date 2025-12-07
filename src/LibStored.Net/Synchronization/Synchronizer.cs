// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Synchronization;

/// <summary>
/// Manages the mapping and synchronization of <see cref="StoreJournal"/> instances and their connections.
/// </summary>
public class Synchronizer
{
    private readonly Dictionary<string, StoreJournal> _stores = [];

    private HashSet<Protocol.ProtocolLayer> _connections = [];

    /// <summary>
    /// Maps a <see cref="SynchronizableStore{TStore}"/> to the synchronizer, enabling tracking and synchronization.
    /// </summary>
    /// <typeparam name="TStore">The type of the store.</typeparam>
    /// <param name="store">The synchronizable store to map.</param>
    public void Map<TStore>(SynchronizableStore<TStore> store) where TStore : Store
        => _stores.Add(store.Store().Hash, store.Journal());

    /// <summary>
    /// Unmaps a <see cref="SynchronizableStore{TStore}"/> from the synchronizer and drops it from all connections.
    /// </summary>
    /// <typeparam name="TStore">The type of the store.</typeparam>
    /// <param name="store">The synchronizable store to unmap.</param>
    public void Unmap<TStore>(SynchronizableStore<TStore> store) where TStore : Store
    {
        _stores.Remove(store.Store().Hash);

        foreach (Protocol.ProtocolLayer connection in _connections)
        {
            if (connection is SyncConnection syncConnection)
            {
                syncConnection.Drop(store.Journal());
            }
        }
    }

    /// <summary>
    /// Create the <see cref="SyncConnection"/> and wrap the layer.
    /// </summary>
    /// <param name="layer">The protocol layer to wrap.</param>
    /// <returns>The created <see cref="SyncConnection"/>.</returns>
    public SyncConnection Connect(Protocol.ProtocolLayer layer)
    {
        SyncConnection connection = new(this, layer);
        _connections.Add(connection);
        return connection;
    }

    /// <summary>
    /// Disconnects and removes the <see cref="SyncConnection"/> associated with the given protocol layer.
    /// </summary>
    /// <param name="layer">The protocol layer to disconnect.</param>
    public void Disconnect(Protocol.ProtocolLayer layer)
    {
        SyncConnection? connection = ToConnection(layer);
        if (connection is null)
        {
            return;
        }

        _connections.Remove(connection);
    }

    /// <summary>
    /// Only create the <see cref="SyncConnection"/> without wrapping any layer.
    /// </summary>
    /// <returns>The created <see cref="SyncConnection"/>.</returns>
    public SyncConnection CreateConnectionLayer()
    {
        SyncConnection connection = new(this);
        _connections.Add(connection);
        return connection;
    }

    /// <summary>
    /// Processes all mapped stores and sends updates to all connections.
    /// </summary>
    public void Process()
    {
        foreach (KeyValuePair<string, StoreJournal> storeJournal in _stores)
        {
            Process(storeJournal.Value);
        }
    }

    /// <summary>
    /// Marks the specified store as a source and initiates synchronization over the given protocol layer.
    /// </summary>
    /// <typeparam name="TStore">The type of the store.</typeparam>
    /// <param name="store">The synchronizable store to synchronize from.</param>
    /// <param name="connection">The protocol layer to use for synchronization.</param>
    public void SyncFrom<TStore>(SynchronizableStore<TStore> store, Protocol.ProtocolLayer connection) where TStore : Store
    {
        StoreJournal? j = ToJournal(store.Store().Hash);
        SyncConnection? c = ToConnection(connection);
        if (j is null || c is null)
        {
            return;
        }

        c.Source(j);
    }

    /// <summary>
    /// Marks the specified store as a source and initiates synchronization over the given <see cref="SyncConnection"/>.
    /// </summary>
    /// <typeparam name="TStore">The type of the store.</typeparam>
    /// <param name="store">The synchronizable store to synchronize from.</param>
    /// <param name="connection">The <see cref="SyncConnection"/> to use for synchronization.</param>
    public void SyncFrom<TStore>(SynchronizableStore<TStore> store, SyncConnection connection) where TStore : Store
    {
        StoreJournal? j = ToJournal(store.Store().Hash);
        SyncConnection? c = _connections.Contains(connection) ? connection : null;
        if (j is null || c is null)
        {
            return;
        }

        c.Source(j);
    }

    /// <summary>
    /// Determines whether the specified journal is being synchronized by any connection except the given one.
    /// </summary>
    /// <param name="journal">The store journal to check.</param>
    /// <param name="notOverConnection">The connection to exclude from the check.</param>
    /// <returns>True if the journal is being synchronized by another connection; otherwise, false.</returns>
    public bool IsSynchronizing(StoreJournal journal, SyncConnection notOverConnection)
    {
        foreach (Protocol.ProtocolLayer protocolLayer in _connections)
        {
            SyncConnection? connection = protocolLayer as SyncConnection;
            if (connection is null)
            {
                continue;
            }

            if (connection != notOverConnection && connection.IsSynchronizing(journal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns if the given store is currently connected over the given connection.
    /// </summary>
    /// <param name="journal"></param>
    /// <param name="connection"></param>
    /// <returns></returns>
    public bool IsConnected(StoreJournal journal, SyncConnection connection)
    {
        SyncConnection? c = _connections.Contains(connection) ? connection : null;
        return c is not null && c.IsConnected(journal);
    }

    internal StoreJournal? ToJournal(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return null;
        }

        return _stores.GetValueOrDefault(hash);
    }

    private void Process(StoreJournal journal)
    {
        foreach (Protocol.ProtocolLayer connection in _connections)
        {
            SyncConnection? sc = connection as SyncConnection;
            sc?.Process(journal);
        }
    }

    /// <summary>
    /// Find the SyncConnection instance from the given connection.
    /// Provide the connection as given to <see cref="Connect"/> before.
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    private SyncConnection? ToConnection(Protocol.ProtocolLayer layer)
    {
        Protocol.ProtocolLayer? c = layer.Up();
        if (c is null)
        {
            return null;
        }

        if (!_connections.Contains(c))
        {
            return null;
        }

        return c as SyncConnection;
    }
}
