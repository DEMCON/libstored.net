// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Synchronization;

public class Synchronizer
{
    private readonly Dictionary<string, StoreJournal> _stores = [];

    private HashSet<Protocol.ProtocolLayer> _connections = [];

    public void Map<TStore>(SynchronizableStore<TStore> store) where TStore : Store
        => _stores.Add(store.Store().Hash, store.Journal());

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
    /// <param name="layer"></param>
    /// <returns></returns>
    public SyncConnection Connect(Protocol.ProtocolLayer layer)
    {
        SyncConnection connection = new(this, layer);
        _connections.Add(connection);
        return connection;
    }

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
    /// Only create the  <see cref="SyncConnection"/> without wrapping any layer.
    /// </summary>
    /// <returns></returns>
    public SyncConnection CreateConnectionLayer()
    {
        SyncConnection connection = new(this);
        _connections.Add(connection);
        return connection;
    }

    public void Process()
    {
        foreach (KeyValuePair<string, StoreJournal> storeJournal in _stores)
        {
            Process(storeJournal.Value);
        }
    }

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