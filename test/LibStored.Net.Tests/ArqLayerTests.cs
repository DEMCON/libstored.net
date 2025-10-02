// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using LibStored.Net.Protocol;

namespace LibStored.Net.Tests;

public class ArqLayerTests
{
    [Fact]
    public void ArqNormalTest()
    {
        LoggingLayer top = new();
        ArqLayer arq = new();
        arq.Wrap(top);
        LoggingLayer bottom = new();
        bottom.Wrap(arq);

        top.Flush();

        Assert.Equal(ProtocolTests.String([0x40]), bottom.Encoded[0]);
        bottom.Decode([0x80, 0x40]);
        Assert.Equal(ProtocolTests.String([0x80]), bottom.Encoded[1]);

        bottom.Decode([0x01, .." 1"u8]);
        Assert.Equal(" 1", top.Decoded[0]);
        Assert.Equal(ProtocolTests.String([0x81]), bottom.Encoded[2]);

        bottom.Decode([0x02, .." 2"u8]);
        Assert.Equal(" 2", top.Decoded[1]);
        Assert.Equal(ProtocolTests.String([0x82]), bottom.Encoded[3]);

        top.Encode(" 3"u8, true);
        Assert.Equal(ProtocolTests.String([0x01, .." 3"u8]), bottom.Encoded[4]);

        bottom.Decode([0x81, 0x03, .." 5"u8]);
        Assert.Equal(" 5", top.Decoded[2]);
        Assert.Equal(ProtocolTests.String([0x83]), bottom.Encoded[5]);

        top.Encode(" 6"u8, true);
        Assert.Equal(ProtocolTests.String([0x02, .." 6"u8]), bottom.Encoded[6]);
    }

    [Fact]
    public void RetransmitTest()
    {
        LoggingLayer top = new();
        ArqLayer arq = new();
        arq.Wrap(top);
        LoggingLayer bottom = new();
        bottom.Wrap(arq);

        bottom.Decode([0xff]);
        // Ignored
        bottom.Decode([0x40]);
        Assert.Equal(ProtocolTests.String([0x80, 0x40]), bottom.Encoded[0]);

        bottom.Decode([0x40]);
        Assert.Equal(ProtocolTests.String([0x80, 0x40]), bottom.Encoded[1]);

        top.Flush();
        // Retransmit
        Assert.Equal(ProtocolTests.String([0x40]), bottom.Encoded[2]);

        bottom.Decode([0x80]);
        top.Flush();
        // No retransmit
        Assert.Equal(3, bottom.Encoded.Count);

        top.Clear();
        bottom.Clear();

        top.Encode(" 1"u8, true);
        Assert.Equal(ProtocolTests.String([0x01, .." 1"u8]), bottom.Encoded[0]);

        top.Encode(" 2"u8, true);
        // Triggers retransmit of 1
        Assert.Equal(ProtocolTests.String([0x01, .." 1"u8]), bottom.Encoded[1]);

        top.Flush();
        // Retransmit
        Assert.Equal(ProtocolTests.String([0x01, .." 1"u8]), bottom.Encoded[2]);

        bottom.Decode([0x81]);
        Assert.Equal(ProtocolTests.String([0x02, .." 2"u8]), bottom.Encoded[3]);

        // Wrong ack
        bottom.Decode([0x83]);
        Assert.Equal(4, bottom.Encoded.Count);
        bottom.Decode([0x82]);

        top.Clear();
        bottom.Clear();

        bottom.Decode([0x01, .." 3"u8]);
        Assert.Equal(ProtocolTests.String([0x81]), bottom.Encoded[0]); // Assume lost

        bottom.Decode([0x01, .." 3"u8]);
        Assert.Equal(ProtocolTests.String([0x81]), bottom.Encoded[1]);

        bottom.Decode([0x02, .." 4"u8]);
        Assert.Equal(ProtocolTests.String([0x82]), bottom.Encoded[2]);
    }

    [Fact]
    public void KeepAliveTest()
    {
        LoggingLayer top = new();
        ArqLayer arq = new();
        arq.Wrap(top);
        LoggingLayer bottom = new();
        bottom.Wrap(arq);

        bottom.Decode([0x80, 0x40]);
        arq.Flush();
        bottom.Clear();

        // No queue, empty message
        arq.KeepAlive();
        Assert.Equal(ProtocolTests.String([0x41]), bottom.Encoded[0]);
        bottom.Decode([0x81]);

        top.Encode(" 1"u8, true);
        Assert.Equal(ProtocolTests.String([0x02, .." 1"u8]), bottom.Encoded[1]);

        arq.KeepAlive();
        Assert.Equal(ProtocolTests.String([0x02, .." 1"u8]), bottom.Encoded[2]); // Retransmit instead of empty message
        bottom.Decode([0x82]);

        bottom.Decode([0x41]);
        Assert.Empty(top.Decoded);
        Assert.Equal(ProtocolTests.String([0x81]), bottom.Encoded[3]); // Retransmit instead of empty message
    }

    [Fact]
    public void EventTest()
    {
        LoggingLayer top = new();
        ArqLayer arq = new(100);
        arq.Wrap(top);
        LoggingLayer bottom = new();
        bottom.Wrap(arq);

        ArqEvent lastEvent = ArqEvent.None;
        arq.EventOccurred += (_, e) => lastEvent = e.Event;

        bottom.Decode([0x80, 0x40]);
        top.Encode(" 1"u8, true);
        bottom.Decode([0x01]);
        bottom.Clear();

        Assert.Equal(ArqEvent.None, lastEvent);
        bottom.Decode([0x40]);
        Assert.Equal(ArqEvent.Reconnect, lastEvent);

        for(int i = 0; i < 5; i++)
        {
            top.Encode("01234567890123456789"u8, true);
        }
        Assert.Equal(ArqEvent.EncodeBufferOverflow, lastEvent);

        for (int i = 0; i < ArqLayer.RetransmitCallbackThreshold; i++)
        {
            top.Flush();
        }
        Assert.Equal(ArqEvent.Retransmit, lastEvent);
    }
}
