// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;

namespace LibStored.Net.Protocol;

/// <summary>
/// Automatic Repeat Request layer
/// </summary>
public class ArqLayer : ProtocolLayer
{
    private const byte NopFlag = 0x40;
    private const byte AckFlag = 0x80;
    private const byte SeqMask = 0x3F;

    private readonly int _maxEncodeBufferSize;
    private Queue<byte[]> _encodeQueue = new();
    private int _encodeQueueBytes;
    private bool _encoding;
    private readonly List<byte> _buffer = [];
    private byte _sendSeq;
    private byte _recvSeq;

    /// <summary>
    /// Create the layer.
    /// Sends a KeepAlive message.
    /// </summary>
    public ArqLayer(int maxEncodeBufferSize = 0)
    {
        _maxEncodeBufferSize = maxEncodeBufferSize;
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
            // Overflow
            // TODO: trigger event
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
                if (WaitingForAck() && (header & SeqMask) == (_encodeQueue.Peek()[0] & SeqMask))
                {
                    PopEncodeQueue();
                    doTransmit = true;

                    if ((header & SeqMask) == 0)
                    {
                        reconnect = true;
                        // Connected()
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
                _recvSeq = NextSeq(0);

                response[responseLen++] = AckFlag;

                if (!reconnect)
                {
                    // TODO:
                    // Inject a reset message in the queue
                    // Reset the send seq
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

        base.Encode(_encodeQueue.Peek(), true);
        return true;
    }

    private void PushEncodeQueue(ReadOnlySpan<byte> buffer)
    {
        byte[] bytes = [_sendSeq, ..buffer];
        PushEncodeQueueRaw(bytes);
    }

    private void PushEncodeQueueRaw(byte[] bytes)
    {
        _encodeQueue.Enqueue(bytes);
        _sendSeq = NextSeq(_sendSeq);
        _encodeQueueBytes += bytes.Length;
    }

    private void PopEncodeQueue()
    {
        byte[] item = _encodeQueue.Peek();
        _encodeQueueBytes -= item.Length;
        _encodeQueue.Dequeue();
    }

    private bool WaitingForAck() => _encodeQueue.Count != 0;

    private byte NextSeq(byte seq)
    {
        byte newSeq = (byte)((seq + 1) & SeqMask);
        return newSeq == 0 ? (byte)1 : newSeq;
    }
}
