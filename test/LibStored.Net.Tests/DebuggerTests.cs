// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace LibStored.Net.Tests;

public class DebuggerTests
{
    [Fact]
    public void CapabilitiesTest()
    {
        Debugger debugger = new();
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "?");
        Assert.Single(logging.Encoded);
        Assert.True(logging.Encoded[0].Length > 1);
    }

    [Theory]
    [InlineData(null, "?")]
    [InlineData("asdf", "asdf")]
    public void IdentificationTest(string? input, string expected)
    {
        Debugger debugger = new();
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        debugger.Identification = input;

        Decode(debugger, "i");
        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Theory]
    [InlineData("", "2")]
    [InlineData("baab", "2 baab")]
    public void VersionTest(string input, string expected)
    {
        Debugger debugger = new();
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        debugger.Versions = input;

        Decode(debugger, "v");
        Assert.Single(logging.Encoded);
        Assert.Equal([expected], logging.Encoded);
    }

    [Fact]
    public void FindTest()
    {
        ExampleStore store = new();
        Debugger debugger = new();
        debugger.Map(store);

        Assert.NotNull(debugger.Find("/number"));
        Assert.Null(debugger.Find("/not number"));
    }
    
    [Fact]
    public void FindMultiTest()
    {
        Debugger debugger = new();
        TestStore store1 = new();
        TestStore store2 = new();
        
        // Invalid names
        debugger.Map(store1, "first");
        Assert.Null(debugger.Find("/default int8"));
        debugger.Map(store1, "/fir/st");
        Assert.Null(debugger.Find("/default int8"));
        
        debugger.Map(store1, "/first");

        DebugVariant? v1 = debugger.Find("/default int8");
        DebugVariant? v2 = debugger.Find("/first/default int8");
        
        Assert.NotNull(v1);
        Assert.NotNull(v2);
        Assert.Equivalent(v1, v2);
        // Check that these variables point to the same store
        Assert.True(v1.Buffer().Overlaps(v2.Buffer()));
        // Abbreviations are not supported
        
        debugger.Map(store2, "/second");
        Assert.Null(debugger.Find("/default int8"));
        DebugVariant? v3 = debugger.Find("/first/default int8");
        DebugVariant? v4 = debugger.Find("/second/default int8");
        Assert.NotNull(v3);
        Assert.NotNull(v4);
        Assert.Equivalent(v3, v4);
        // Check that these variables point to a different store
        Assert.False(v3.Buffer().Overlaps(v4.Buffer()));
        
        v3 = debugger.Find("/f/default int8");
        v4 = debugger.Find("/s/default int8");
        Assert.NotNull(v3);
        Assert.NotNull(v4);
    }
    
    [Fact]
    public void ListTest()
    {
        Debugger debugger = new();
        ExampleStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "l");
        Assert.Single(logging.Encoded);
        Assert.Equal(["3b4/number\n2f8/fraction\n02f/text\n381/four ints[0]\n381/four ints[1]\n381/four ints[3]\n381/four ints[4]\n"], logging.Encoded);
    }
    
    [Fact]
    public void ListMultiTest()
    {
        Debugger debugger = new();
        TestStore store1 = new();
        TestStore store2 = new();
        debugger.Map(store1, "/first");
        debugger.Map(store2, "/second");

        List<string> names = [];
        debugger.List((name, variant) => names.Add(name));
        
        Assert.True(names.Count > 10);
        Assert.Contains("/first/default int8", names);
        Assert.Contains("/second/array bool[1]", names);
    }

    [Fact]
    public void EchoTest()
    {
        Debugger debugger = new();
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "eHello World!");
        Assert.Single(logging.Encoded);
        Assert.Equal("Hello World!", logging.Encoded[0]);
    }

    [Fact]
    public void ReadTest()
    {
        Debugger debugger = new();
        ExampleStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        store.Number.Set(42);
        Span<byte> buffer = stackalloc byte[store.Number.Size];
        BinaryPrimitives.WriteInt32BigEndian(buffer, store.Number.Get());
        string expected = Convert.ToHexStringLower(buffer.TrimStart((byte)0b0));

        store.Fraction.Set(3.14);
        Span<byte> bufferFaction = stackalloc byte[store.Fraction.Size];
        BinaryPrimitives.WriteDoubleBigEndian(bufferFaction, store.Fraction.Get());
        string expectedFactionHex = Convert.ToHexStringLower(bufferFaction.TrimStart((byte)0b0));

        Decode(debugger, "r/number");
        Assert.Single(logging.Encoded);
        Assert.Equal(expected, logging.Encoded[0]);
    }

    [Fact]
    public void WriteTest()
    {
        Debugger debugger = new();
        ExampleStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "w12345678/number");
        int number = store.Number.Get();
        Assert.Equal(0x12345678, number);
        Assert.Single(logging.Encoded);
        Assert.Equal("!", logging.Encoded[0]);
    }

    [Theory]
    [InlineData("/number", "2a")]
    [InlineData("/fraction", "40091eb851eb851f")]
    [InlineData("/text", "48656c6c6f20576f726c6421000000")]
    public void ReadValueTest(string path, string expected)
    {
        Debugger debugger = new();
        ExampleStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        store.Number.Set(42);
        store.Fraction.Set(3.14);
        store.Text.Set("Hello World!"u8);

        ReadOnlySpan<byte> text = store.Text.Get();

        byte[] bytes = Encoding.UTF8.GetBytes("Hello World!\0\0\0");
        string expectedHex = Convert.ToHexStringLower(bytes);

        Decode(debugger, $"r{path}");
        Assert.Single(logging.Encoded);
        Assert.Equal(expected, logging.Encoded[0]);
    }


    private void Encode(Debugger layer, string data, bool last = true)
    {
        byte[] bytes = DebuggerTests.Bytes(data);
        layer.Encode(bytes, last);
    }

    private void Decode(Debugger layer, string data)
    {
        byte[] bytes = DebuggerTests.Bytes(data);
        layer.Decode(bytes);
    }

    private static byte[] Bytes(string data) => Encoding.ASCII.GetBytes(data);
    private static string String(byte[] data) => Encoding.ASCII.GetString(data);
}