// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

/// <summary>
/// Event for the ARQ layer
/// </summary>
public enum ArqEvent
{
    /// <summary>
    /// None
    /// </summary>
    None,
    /// <summary>
    ///  An unexpected reset message has been received.
    /// </summary>
    Reconnect,
    /// <summary>
    /// The maximum buffer capacity has passed.
    /// </summary>
    EncodeBufferOverflow,
    /// <summary>
    /// <see cref="ArqLayer.RetransmitCallbackThreshold"/> has been reached on the current message.
    /// </summary>
    Retransmit
}

/// <summary>
/// Provides data for events related to ARQ (Automatic Repeat reQuest) operations.
/// </summary>
public class ArqEventArgs : EventArgs
{
    /// <summary>
    /// Event
    /// </summary>
    public ArqEvent Event { get; }

    /// <summary>
    /// Initializes a new instance of the ArqEventArgs class with the specified event information.
    /// </summary>
    /// <param name="evt">The event data associated with this instance.</param>
    public ArqEventArgs(ArqEvent evt) => Event = evt;
}

/// <summary>
/// Automatic Repeat Request layer
/// </summary>
public class ArqLayer : ProtocolLayer
{
    /// <summary>
    /// Number of successive retransmits before the event is emitted.
    /// </summary>
    public const int RetransmitCallbackThreshold = 10;

    private const byte NopFlag = 0x40;
    private const byte AckFlag = 0x80;
    private const byte SeqMask = 0x3F;

    private readonly int _maxEncodeBufferSize;
    private readonly LinkedList<byte[]> _encodeQueue = new();
    private readonly List<byte> _buffer = [];

    private int _encodeQueueBytes;
    private bool _encoding;
    private byte _sendSeq;
    private byte _recvSeq;
    private byte _retransmits;

    /// <summary>
    /// Create the layer.
    /// Sends a KeepAlive message.
    /// </summary>
    public ArqLayer(int maxEncodeBufferSize = 0)
    {
        _maxEncodeBufferSize = maxEncodeBufferSize;

        // Empty encode with seq 0, which indicates a reset message.
        KeepAlive();
    }

    /// <summary>
    ///
    /// </summary>
    public event EventHandler<ArqEventArgs>? EventOccurred;

    /// <inheritdoc />
    public override void Reset()
    {
        while (_encodeQueue.Count > 0)
        {
            PopEncodeQueue();
        }

        _encoding = false;
        _retransmits = 0;
        _sendSeq = 0;
        _recvSeq = 0;
        _buffer.Clear();

        base.Reset();

        KeepAlive();
    }

    /// <inheritdoc/>
    public override bool Flush()
    {
        bool res = !Transmit();
        return base.Flush() && res;
    }

    /// <inheritdoc />
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        if (_maxEncodeBufferSize > 0 && _maxEncodeBufferSize < _encodeQueueBytes + buffer.Length + 1 )
        {
            Event(ArqEvent.EncodeBufferOverflow);
        }

        if (!_encoding)
        {
            if (last)
            {
                PushEncodeQueue(buffer);
            }
            else
            {
                _buffer.AddRange(buffer);
                _encoding = true;
            }
        }
        else
        {
            _buffer.AddRange(buffer);
            if (last)
            {
                Span<byte> b= CollectionsMarshal.AsSpan(_buffer);
                PushEncodeQueue(b);
                _buffer.Clear();
                _encoding = false;
            }
        }

        Transmit();
    }

    /// <inheritdoc/>
    public override void Decode(Span<byte> buffer)
    {
        bool reconnect = false;

        Span<byte> response = [0, 0];
        byte responseLen = 0;
        bool doTransmit = false;
        bool doDecode = false;

        while (buffer.Length > 0)
        {
            byte header = buffer[0];
            if ((header & AckFlag) != 0)
            {
                if (WaitingForAck() && (header & SeqMask) == (_encodeQueue.First()[0] & SeqMask))
                {
                    PopEncodeQueue();
                    _retransmits = 0;

                    doTransmit = true;

                    if ((header & SeqMask) == 0)
                    {
                        reconnect = true;
                        // TODO: Add Connected() or new Reconnected event
                    }
                }

                buffer = buffer.Slice(1);
            }
            else if((header & SeqMask) == _recvSeq)
            {
                // The next message
                response[responseLen++] = (byte) (_recvSeq | AckFlag);
                _recvSeq = NextSeq(_recvSeq);

                doDecode = (header & NopFlag) == 0;
                doTransmit = true;

                buffer = buffer.Slice(1);
            }
            else if ((header & SeqMask) == 0)
            {
                // Unexpected reset
                Event(ArqEvent.Reconnect);

                // Send ack
                _recvSeq = NextSeq(0);
                response[responseLen++] = AckFlag;

                // Also reset our send seq.
                if (!reconnect && (_encodeQueueBytes == 0 || _encodeQueue.First()[0] != NopFlag))
                {
                    // Insert at front of queue.
                    PushEncodeQueueRaw([NopFlag], false);
                    _sendSeq = NextSeq(0);

                    // Re-encode existing outbound messages.
                    foreach (byte[] bytes in _encodeQueue.Skip(1))
                    {
                        bytes[0] = (byte)((bytes[0] & ~SeqMask) | _sendSeq);
                        _sendSeq = NextSeq(_sendSeq);
                    }
                }

                doTransmit = true;
                buffer = buffer.Slice(1);
            }
            else if(NextSeq((byte)(header & SeqMask)) == _recvSeq)
            {
                // Retransmit, send an Ack again
                response[responseLen++] = (byte) (header & SeqMask | AckFlag);

                if ((header & NopFlag) != 0)
                {
                    buffer = buffer.Slice(1);
                }
                else
                {
                    // Drop remaining
                    buffer = Span<byte>.Empty;
                }
            }
            else
            {
                // Drop
                buffer = Span<byte>.Empty;
                doTransmit = true;
            }

            if (doDecode)
            {
                break;
            }

            if (responseLen == response.Length)
            {
                // Buffer full. Drop and wait for retransmit.
                break;
            }
        }

        if (doDecode)
        {
            // TODO add transmit pause logic
            base.Decode(buffer);
        }

        if (responseLen > 0)
        {
            base.Encode(response.Slice(0,responseLen), !doTransmit);
        }

        if (doTransmit)
        {
            if (!Transmit() && responseLen > 0)
            {
                base.Encode([], true);
            }
        }
    }

    /// <summary>
    /// Send a keep-alive packet to check the connection.
    /// </summary>
    public void KeepAlive()
    {
        if (_encodeQueue.Count == 0)
        {
            byte nop = (byte)(_sendSeq | NopFlag);
            PushEncodeQueueRaw([nop]);
        }

        Transmit();
    }

    /// <summary>
    /// Transmit the first message in the encode queue.
    /// </summary>
    /// <returns>True when a message was sent.</returns>
    private bool Transmit()
    {
        if (_encodeQueue.Count == 0)
        {
            return false;
        }

        if (_retransmits < byte.MaxValue)
        {
            _retransmits++;
        }

        if (_retransmits >= RetransmitCallbackThreshold)
        {
            Event(ArqEvent.Retransmit);
        }

        Debug.Assert(WaitingForAck());
        base.Encode(_encodeQueue.First(), true);
        return true;
    }

    private void PushEncodeQueue(ReadOnlySpan<byte> buffer)
    {
        byte[] bytes = [_sendSeq, ..buffer];
        PushEncodeQueueRaw(bytes);
    }

    private void PushEncodeQueueRaw(byte[] bytes, bool back = true)
    {
        if (back)
        {
            _encodeQueue.AddLast(bytes);
        }
        else
        {
            _encodeQueue.AddFirst(bytes);
        }

        _sendSeq = NextSeq(_sendSeq);
        _encodeQueueBytes += bytes.Length;
    }

    private void PopEncodeQueue()
    {
        byte[] item = _encodeQueue.First();
        _encodeQueueBytes -= item.Length;
        _encodeQueue.RemoveFirst();
    }

    private bool WaitingForAck() => _encodeQueue.Count != 0;

    private byte NextSeq(byte seq)
    {
        byte newSeq = (byte)((seq + 1) & SeqMask);
        return newSeq == 0 ? (byte)1 : newSeq;
    }

    private void Event(ArqEvent @event) => EventOccurred?.Invoke(this, new ArqEventArgs(@event));
}
