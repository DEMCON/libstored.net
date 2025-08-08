// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using NetMQ;
using NetMQ.Sockets;

namespace LibStored.Net.ZeroMQ;

public class DebugZeroMQLayer : ZeroMQLayer
{
    public DebugZeroMQLayer(ResponseSocket socket) : base(socket) { }
}

public class SyncZeroMQLayer : ZeroMQLayer
{
    /// <summary>
    /// For libstored higher than v1.7.1
    /// </summary>
    /// <param name="socket"></param>
    public SyncZeroMQLayer(DealerSocket socket) : base(socket) { }

    /// <summary>
    /// For libstored upto v1.7.1
    /// </summary>
    /// <param name="socket"></param>
    public SyncZeroMQLayer(PairSocket socket) : base(socket) { }
}

public class ZeroMQLayer : Protocol.ProtocolLayer, IDisposable
{
    private readonly NetMQSocket _socket;
    private List<byte> _receiveBuffer = [];

    public ZeroMQLayer(NetMQSocket socket)
    {
        _socket = socket;
    }

    public int ReceiveAll(TimeSpan timeout = default)
    {
        bool first = true;
        int receivedBytes = 0;
        bool more = false;
        do
        {
            receivedBytes += Receive(first ? timeout : TimeSpan.Zero, out more);
            first = false;
        } while (more);

        return receivedBytes;
    }

    public int Receive(TimeSpan timeout, out bool hasMore)
    {
        int receivedBytes = 0;
        Msg msg = new();
        msg.InitEmpty();
        hasMore = false;
        if (_socket.TryReceive(ref msg, timeout))
        {
            hasMore = msg.HasMore;
            receivedBytes = msg.Size;
            if (msg.HasMore || _receiveBuffer.Count > 0)
            {
                _receiveBuffer.AddRange(msg.Slice());
            }
            else if (!msg.HasMore)
            {
                // Single message, decode it immediately
                Decode(msg.Slice());
            }

            if (!msg.HasMore && _receiveBuffer.Count > 0)
            {
                Span<byte> buffer = CollectionsMarshal.AsSpan(_receiveBuffer);
                Decode(buffer);
                _receiveBuffer.Clear();
            }
        }

        return receivedBytes;
    }

    /// <inheritdoc />l
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        Msg msg = new();
        msg.InitPool(buffer.Length);

        buffer.CopyTo(msg.Slice());

        _socket.Send(ref msg, !last);
        msg.Close();

        base.Encode(buffer, last);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _socket.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}