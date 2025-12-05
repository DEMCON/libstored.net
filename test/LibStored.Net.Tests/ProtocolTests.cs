// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Text;
using LibStored.Net.Protocol;
using Microsoft.Extensions.Logging.Abstractions;

namespace LibStored.Net.Tests;

public class ProtocolTests
{
    [Fact]
    public void LoggingLayerTest()
    {
        LoggingLayer logging = new();

        byte[] bytes = Enumerable.Range(0, 255).Select(x => (byte)x).ToArray();

        logging.Encode(bytes, true);

        Assert.Single(logging.Encoded);
        Assert.Equal([String(bytes)], logging.Encoded);
        Assert.Equal(bytes, Bytes(logging.Encoded[0]));
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("123\0", "123\u007f@")]
    [InlineData("123\r4", "123\u007fM4")]
    [InlineData("123\u007f", "123\u007f\u007f")]
    [InlineData("\u007f123\r", "\u007f\u007f123\u007f\u004d")]
    public void AsciiEncodeTest(string input, string expected)
    {
        AsciiEscapeLayer ascii = new();
        LoggingLayer logging = new();
        logging.Wrap(ascii);

        Encode(ascii, input);

        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("1", "1")]
    [InlineData("\u007fM", "\r")]
    [InlineData("\u007f@", "\0")]
    [InlineData("\u007fQ", "\u0011")]
    [InlineData("\u007fS", "\u0013")]
    [InlineData("\u007f[", "\u001b")]
    [InlineData("\u007f\u007f", "\u007f")]
    public void AsciiEncodeSingleTest(string expected, string input)
    {
        AsciiEscapeLayer ascii = new();
        LoggingLayer logging = new();
        logging.Wrap(ascii);

        Encode(ascii, input);

        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("123\u0006", "123\u007fF")]
    [InlineData("123\u007f", "123\u007f")]
    [InlineData("\u0001123", "\u007fA12\r3")]
    [InlineData("123\r4", "123\u007fM4")]
    [InlineData("123\u007f", "123\u007f\u007f")]
    [InlineData("\u007f123\r", "\u007f\u007f123\u007f\u004d")]
    public void AsciiDecodeTest(string expected, string input)
    {
        AsciiEscapeLayer ascii = new();
        LoggingLayer logging = new();
        ascii.Wrap(logging);

        Decode(ascii, input);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
    }

    [Theory]
    [InlineData( "\u001b_123\u001b\\", "123")]
    [InlineData( "\u001b_123\u001b\\", "1", "2", "3")]
    public void TerminalEncodeTest(string expected, params string[] inputs)
    {
        TerminalLayer term = new(NullLogger<TerminalLayer>.Instance);
        LoggingLayer logging = new();
        logging.Wrap(term);

        for (int i = 0; i < inputs.Length; i++)
        {
            string input = inputs[i];
            Encode(term, input, i == inputs.Length - 1);
        }

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

        TerminalLayer term = new(NullLogger<TerminalLayer>.Instance);
        LoggingLayer logging = new();
        term.Wrap(logging);

        Decode(term, input);

        writer.Flush();
        byte[] debug = ms.ToArray();
        string debugMessage = String(debug);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
        Assert.Equal(expectedDebug, debugMessage);
    }

    [Theory]
    [InlineData("abcdef", "123456", "123\u001b_abc", "def\u001b\\456")]
    [InlineData("abc", "12", "1","\u001b","_","a", "b", "c", "\u001b","\\", "2")]
    public void TerminalSplitDecodeTest(string expected, string expectedDebug, params string[] inputs)
    {
        using MemoryStream ms = new();
        using StreamWriter writer = new(ms);
        Console.SetOut(writer);

        TerminalLayer term = new(NullLogger<TerminalLayer>.Instance);
        LoggingLayer logging = new();
        term.Wrap(logging);

        foreach (string input in inputs)
        {
            Decode(term, input);
        }

        writer.Flush();
        byte[] debug = ms.ToArray();
        string debugMessage = String(debug);

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
        Crc8Layer crc8 = new();
        LoggingLayer logging = new();
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
        Crc8Layer crc8 = new();
        LoggingLayer logging = new();
        crc8.Wrap(logging);

        Decode(crc8, input);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
    }

    [Theory]
    [InlineData("1234\xfc")]
    public void Crc8DecodeFailsTest(string input)
    {
        Crc8Layer crc8 = new();
        LoggingLayer logging = new();
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
        Crc16Layer crc16 = new();
        LoggingLayer logging = new();
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
        Crc16Layer crc16 = new();
        LoggingLayer logging = new();
        crc16.Wrap(logging);

        Decode(crc16, input);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
    }

    [Theory]
    [InlineData("1234\x1c\x84")]
    public void Crc16DecodeFailsTest(string input)
    {
        Crc16Layer crc16 = new();
        LoggingLayer logging = new();
        crc16.Wrap(logging);

        Decode(crc16, input);

        Assert.Empty(logging.Decoded);
    }

    [Theory]
    [InlineData("", "00000000")]
    [InlineData("1", "3183DCEFB7")]
    [InlineData("12", "31324F5344CD")]
    [InlineData("123", "313233884863D2")]
    [InlineData("1234", "313233349BE3E0A3")]
    public void Crc32EncodeTest(string input, string expectedHex)
    {
        byte[] expected = Convert.FromHexString(expectedHex);
        string expectedString = String(expected);

        Crc32Layer crc32 = new();
        LoggingLayer logging = new();
        logging.Wrap(crc32);

        Encode(crc32, input);

        Assert.Single(logging.Encoded);
        Assert.Equal([expectedString], logging.Encoded);
    }

    [Theory]
    [InlineData("", "00000000")]
    [InlineData("1", "3183DCEFB7")]
    [InlineData("12", "31324F5344CD")]
    [InlineData("123", "313233884863D2")]
    [InlineData("1234", "313233349BE3E0A3")]
    public void Crc32DecodeTest(string expected, string inputHex)
    {
        byte[] input = Convert.FromHexString(inputHex);

        Crc32Layer crc32 = new();
        LoggingLayer logging = new();
        crc32.Wrap(logging);

        crc32.Decode(input);

        Assert.Single(logging.Decoded);
        Assert.Equal([expected], logging.Decoded);
    }

    [Theory]
    [InlineData("123", "123")]
    [InlineData("123", "12", "3")]
    [InlineData("123", "12", "3", "")]
    public void BufferLayerEncodeTest(string expected, params string[] inputs)
    {
        BufferLayer bufferLayer = new();
        LoggingLayer logging = new();
        logging.Wrap(bufferLayer);

        for (int i = 0; i < inputs.Length; i++)
        {
            string input = inputs[i];
            Encode(bufferLayer, input, i == inputs.Length - 1);
        }

        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("123E", "123")]
    [InlineData("E", "")]
    [InlineData("1234567E", "1234567")]
    [InlineData("1234567E","1234", "567")]
    [InlineData("1234567E", "1234", "567", "")]
    public void SegmentationLayerEncode8Test(string expected, params string[] input)
    {
        SegmentationLayer segmentation = new(8);
        LoggingLayer logging = new();
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
        SegmentationLayer segmentation = new(4);
        LoggingLayer logging = new();
        logging.Wrap(segmentation);

        Encode(segmentation, input);

        Assert.Equal(expected, logging.Encoded);
    }

    [Fact]
    public void SegmentationLayerEncodeMultipleTest()
    {
        SegmentationLayer segmentation = new(4);
        LoggingLayer logging = new();
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
        LoggingLayer logging = new();
        SegmentationLayer segmentation = new(4);
        segmentation.Wrap(logging);

        Decode(segmentation, input);

        Assert.Equal(expected, logging.Decoded);
    }

    [Fact]
    public void SegmentationLayerDecodeEmptyTest()
    {
        LoggingLayer logging = new();
        SegmentationLayer segmentation = new(4);
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
        LoggingLayer logging = new();
        SegmentationLayer segmentation = new(4);
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
        LoggingLayer loggingA = new();
        LoggingLayer loggingB = new();
        LoopbackLayer _ = new(loggingA, loggingB);

        Encode(loggingA, "Hello ", false);
        Encode(loggingB, "other text", false);
        Encode(loggingA, "world!", true);
        Encode(loggingB, "!", true);

        Assert.Equal(["Hello world!"], loggingB.Decoded);
        Assert.Equal(["other text!"], loggingA.Decoded);
    }

    [Fact]
    public void IdleLayerTest()
    {
        IdleCheckLayer idle = new();
        Assert.True(idle.Idle);

        Encode(idle, "down");
        Assert.False(idle.Idle);
        Assert.True(idle.IdleUp);
        Assert.False(idle.IdleDown);

        Decode(idle, "up");
        Assert.False(idle.IdleUp);

        idle.SetIdle();
        Assert.True(idle.Idle);
    }

    private void Encode(ProtocolLayer layer, string data, bool last = true)
    {
        byte[] bytes = Bytes(data);
        layer.Encode(bytes, last);
    }

    private void Decode(ProtocolLayer layer, string data)
    {
        byte[] bytes = Bytes(data);
        layer.Decode(bytes);
    }

    internal static byte[] Bytes(string data) => Encoding.Latin1.GetBytes(data);
    internal static string String(byte[] data) => Encoding.Latin1.GetString(data);
}
