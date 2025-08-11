// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using NetMQ;
using NetMQ.Sockets;

namespace LibStored.Net.ZeroMQ;

/// <summary>
/// Provides a debug implementation of <see cref="ZeroMQLayer"/> using a <see cref="ResponseSocket"/>.
/// </summary>
public class DebugZeroMQLayer : ZeroMQLayer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DebugZeroMQLayer"/> class with the specified response socket.
    /// </summary>
    /// <param name="socket">The response socket to use for communication.</param>
    public DebugZeroMQLayer(ResponseSocket socket) : base(socket) { }
}

/// <summary>
/// Provides a synchronization implementation of <see cref="ZeroMQLayer"/> for different socket types.
/// </summary>
public class SyncZeroMQLayer : ZeroMQLayer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SyncZeroMQLayer"/> class for libstored higher than v1.7.1.
    /// </summary>
    /// <param name="socket">The dealer socket to use for communication.</param>
    public SyncZeroMQLayer(DealerSocket socket) : base(socket) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncZeroMQLayer"/> class for libstored up to v1.7.1.
    /// </summary>
    /// <param name="socket">The pair socket to use for communication.</param>
    public SyncZeroMQLayer(PairSocket socket) : base(socket) { }
}

/// <summary>
/// Represents a ZeroMQ-based protocol layer for sending and receiving messages using NetMQ sockets.
/// </summary>
public class ZeroMQLayer : Protocol.ProtocolLayer, IDisposable
{
    private readonly NetMQSocket _socket;
    private List<byte> _receiveBuffer = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ZeroMQLayer"/> class with the specified NetMQ socket.
    /// </summary>
    /// <param name="socket">The NetMQ socket to use for communication.</param>
    public ZeroMQLayer(NetMQSocket socket)
    {
        _socket = socket;
    }

    /// <summary>
    /// Receives all available messages from the socket within the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout for the first receive operation.</param>
    /// <returns>The total number of bytes received.</returns>
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

    /// <summary>
    /// Receives a single message from the socket within the specified timeout.
    /// </summary>
    /// <param name="timeout">The timeout for the receive operation.</param>
    /// <param name="hasMore">Outputs whether more message parts are available.</param>
    /// <returns>The number of bytes received.</returns>
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

    /// <inheritdoc />
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        Msg msg = new();
        msg.InitPool(buffer.Length);

        buffer.CopyTo(msg.Slice());

        _socket.Send(ref msg, !last);
        msg.Close();

        base.Encode(buffer, last);
    }

    /// <summary>
    /// Releases the resources used by the <see cref="ZeroMQLayer"/>.
    /// </summary>
    /// <param name="disposing">True to release managed resources; otherwise, false.</param>
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
