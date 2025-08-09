// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibStored.Net.Tests;

public class ProtocolTests
{
    [Fact]
    public void LoggingLayerTest()
    {
        Protocol.LoggingLayer logging = new();

        byte[] bytes = Enumerable.Range(0, 255).Select(x => (byte)x).ToArray();

        logging.Encode(bytes, true);

        Assert.Single(logging.Encoded);
        Assert.Equal([ProtocolTests.String(bytes)], logging.Encoded);
        Assert.Equal(bytes, ProtocolTests.Bytes(logging.Encoded[0]));
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("123\r4", "123\u007fM4")]
    [InlineData("123\u007f", "123\u007f\u007f")]
    [InlineData("\u007f123\r", "\u007f\u007f123\u007f\u004d")]
    public void AsciiEncodeTest(string input, string expected)
    {
        Protocol.AsciiEscapeLayer ascii = new();
        Protocol.LoggingLayer logging = new();
        logging.Wrap(ascii);

        Encode(ascii, input);

        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("123\r4", "123\u007fM4")]
    [InlineData("123\u007f", "123\u007f\u007f")]
    [InlineData("\u007f123\r", "\u007f\u007f123\u007f\u004d")]
    public void AsciiDecodeTest(string expected, string input)
    {
        Protocol.AsciiEscapeLayer ascii = new();
        Protocol.LoggingLayer logging = new();
        ascii.Wrap(logging);

        Decode(ascii, input);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
    }

    [Theory]
    [InlineData("123", "\u001b_123\u001b\\")]
    public void TerminalEncodeTest(string input, string expected)
    {
        Protocol.TerminalLayer term = new(NullLogger<Protocol.TerminalLayer>.Instance);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(term);

        Encode(term, input);

        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("123", "\u001b_123\u001b\\", "")]
    [InlineData("123", "\u001b_12\r3\u001b\\", "")] // Remove additional '\r' in message
    [InlineData("flowers", "from the \u001b_flowers\u001b\\...", "from the ...")]
    public void TerminalDecodeTest(string expected, string input, string expectedDebug)
    {
        using MemoryStream ms = new();
        using StreamWriter writer = new(ms);
        Console.SetOut(writer);

        Protocol.TerminalLayer term = new(NullLogger<Protocol.TerminalLayer>.Instance);
        Protocol.LoggingLayer logging = new();
        term.Wrap(logging);

        Decode(term, input);

        writer.Flush();
        byte[] debug = ms.ToArray();
        string debugMessage = ProtocolTests.String(debug);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
        Assert.Equal(expectedDebug, debugMessage);
    }

    [Fact]
    public void TerminalSplitDecodeTest()
    {
        string inputFst = "123\u001b_abc";
        string inputSnd = "def\u001b\\456";

        string expected = "abcdef";
        string expectedDebug = "123456";

        using MemoryStream ms = new();
        using StreamWriter writer = new(ms);
        Console.SetOut(writer);

        Protocol.TerminalLayer term = new(NullLogger<Protocol.TerminalLayer>.Instance);
        Protocol.LoggingLayer logging = new();
        term.Wrap(logging);

        Decode(term, inputFst);
        Decode(term, inputSnd);

        writer.Flush();
        byte[] debug = ms.ToArray();
        string debugMessage = ProtocolTests.String(debug);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
        Assert.Equal(expectedDebug, debugMessage);
    }

    [Theory]
    [InlineData("", "\xff")]
    [InlineData("1", "1\x5e")]
    [InlineData("12", "12\x54")]
    [InlineData("123", "123\xfc")]
    public void Crc8EncodeTest(string input, string expected)
    {
        Protocol.Crc8Layer crc8 = new();
        Protocol.LoggingLayer logging = new();
        logging.Wrap(crc8);

        Encode(crc8, input);

        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("", "\xff")]
    [InlineData("1", "1\x5e")]
    [InlineData("12", "12\x54")]
    [InlineData("123", "123\xfc")]
    public void Crc8DecodeTest(string expected, string input)
    {
        Protocol.Crc8Layer crc8 = new();
        Protocol.LoggingLayer logging = new();
        crc8.Wrap(logging);

        Decode(crc8, input);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
    }

    [Theory]
    [InlineData("1234\xfc")]
    public void Crc8DecodeFailsTest(string input)
    {
        Protocol.Crc8Layer crc8 = new();
        Protocol.LoggingLayer logging = new();
        crc8.Wrap(logging);

        Decode(crc8, input);

        Assert.Empty(logging.Decoded);
    }

    [Theory]
    [InlineData("", "\xff\xff")]
    [InlineData("1", "1\x49\xd6")]
    [InlineData("12", "12\x77\xa2")]
    [InlineData("123", "123\x1c\x84")]
    public void Crc16EncodeTest(string input, string expected)
    {
        Protocol.Crc16Layer crc16 = new();
        Protocol.LoggingLayer logging = new();
        logging.Wrap(crc16);

        Encode(crc16, input);

        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("", "\xff\xff")]
    [InlineData("1", "1\x49\xd6")]
    [InlineData("12", "12\x77\xa2")]
    [InlineData("123", "123\x1c\x84")]
    public void Crc16DecodeTest(string expected, string input)
    {
        Protocol.Crc16Layer crc16 = new();
        Protocol.LoggingLayer logging = new();
        crc16.Wrap(logging);

        Decode(crc16, input);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
    }

    [Theory]
    [InlineData("1234\x1c\x84")]
    public void Crc16DecodeFailsTest(string input)
    {
        Protocol.Crc16Layer crc16 = new();
        Protocol.LoggingLayer logging = new();
        crc16.Wrap(logging);

        Decode(crc16, input);

        Assert.Empty(logging.Decoded);
    }

    [Theory]
    [InlineData("123E", "123")]
    [InlineData("E", "")]
    [InlineData("1234567E", "1234567")]
    [InlineData("1234567E","1234", "567")]
    [InlineData("1234567E", "1234", "567", "")]
    public void SegmentationLayerEncode8Test(string expected, params string[] input)
    {
        Protocol.SegmentationLayer segmentation = new(8);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(segmentation);

        for (int i = 0; i < input.Length; i++)
        {
            Encode(segmentation, input[i], i == input.Length - 1);
        }

        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("1234", "123C", "4E")]
    [InlineData("1234567890", "123C", "456C", "789C", "0E")]
    public void SegmentationLayerEncodeTest(string input, params string[] expected)
    {
        Protocol.SegmentationLayer segmentation = new(4);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(segmentation);

        Encode(segmentation, input);

        Assert.Equal(expected, logging.Encoded);
    }

    [Fact]
    public void SegmentationLayerEncodeMultipleTest()
    {
        Protocol.SegmentationLayer segmentation = new(4);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(segmentation);

        Encode(segmentation, "12345", false);
        Encode(segmentation, "67", false);
        Encode(segmentation, "89", false);
        Encode(segmentation, "", true);

        Assert.Equal(["123C", "456C", "789E"], logging.Encoded);
    }

    [Theory]
    [InlineData("1234E", "1234")]
    [InlineData("E", "")]
    public void SegmentationLayerDecodeTest(string input, params string[] expected)
    {
        Protocol.LoggingLayer logging = new();
        Protocol.SegmentationLayer segmentation = new(4);
        segmentation.Wrap(logging);

        Decode(segmentation, input);

        Assert.Equal(expected, logging.Decoded);
    }

    [Fact]
    public void SegmentationLayerDecodeEmptyTest()
    {
        Protocol.LoggingLayer logging = new();
        Protocol.SegmentationLayer segmentation = new(4);
        segmentation.Wrap(logging);

        Decode(segmentation, "");

        Assert.Empty(logging.Decoded);
    }

    [Theory]
    [InlineData("12345", "12345E")]
    [InlineData("1234567890", "1234567890E")]
    [InlineData("123456789", "123C", "456789E")]
    [InlineData("123456789", "123C", "456789C", "E")]
    public void SegmentationLayerDecodeMultiTest(string expected, params string[] inputs)
    {
        Protocol.LoggingLayer logging = new();
        Protocol.SegmentationLayer segmentation = new(4);
        segmentation.Wrap(logging);

        foreach (string input in inputs)
        {
            Decode(segmentation, input);
        }

        Assert.Equal([expected], logging.Decoded);
    }
    
    
    [Fact]
    public void LoopBackTest()
    {
        Protocol.LoggingLayer loggingA = new();
        Protocol.LoggingLayer loggingB = new();
        Protocol.LoopbackLayer _ = new(loggingA, loggingB);

        Encode(loggingA, "Hello ", false);
        Encode(loggingB, "other text", false);
        Encode(loggingA, "world!", true);
        Encode(loggingB, "!", true);

        Assert.Equal(["Hello world!"], loggingB.Decoded);
        Assert.Equal(["other text!"], loggingA.Decoded);
    }

    private void Encode(Protocol.ProtocolLayer layer, string data, bool last = true)
    {
        byte[] bytes = ProtocolTests.Bytes(data);
        layer.Encode(bytes, last);
    }

    private void Decode(Protocol.ProtocolLayer layer, string data)
    {
        byte[] bytes = ProtocolTests.Bytes(data);
        layer.Decode(bytes);
    }

    private static byte[] Bytes(string data) => Encoding.Latin1.GetBytes(data);
    private static string String(byte[] data) => Encoding.Latin1.GetString(data);
}