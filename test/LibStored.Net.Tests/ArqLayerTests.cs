// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Tests;

public class ArqLayerTests
{
    [Fact]
    public void ArqNormalTest()
    {
        Protocol.LoggingLayer top = new();
        Protocol.ArqLayer arq = new();
        arq.Wrap(top);
        Protocol.LoggingLayer bottom = new();
        bottom.Wrap(arq);

        top.Flush();
        bottom.Decode([0x80, 0x40]);
        // TODO: ack this and test the ack is send

        string expected = ProtocolTests.String([0x40]);
        Assert.Equal([expected], bottom.Encoded);
    }
}
