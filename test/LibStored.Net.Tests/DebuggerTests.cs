// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using LibStored.Net.Debugging;

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
        Assert.Equal(["3b4/ExampleStore/number\n2f8/ExampleStore/fraction\n02f/ExampleStore/text\n381/ExampleStore/four ints[0]\n381/ExampleStore/four ints[1]\n381/ExampleStore/four ints[3]\n381/ExampleStore/four ints[4]\n"], logging.Encoded);
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
        Span<byte> bufferFraction = stackalloc byte[store.Fraction.Size];
        BinaryPrimitives.WriteDoubleBigEndian(bufferFraction, store.Fraction.Get());
        string expectedFractionHex = Convert.ToHexStringLower(bufferFraction.TrimStart((byte)0b0));

        Decode(debugger, "r/number");
        Assert.Single(logging.Encoded);
        Assert.Equal(expected, logging.Encoded[0]);

        Decode(debugger, "r/fraction");
        Assert.Equal(expectedFractionHex, logging.Encoded[1]);
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

        Decode(debugger, $"r{path}");
        Assert.Single(logging.Encoded);
        Assert.Equal(expected, logging.Encoded[0]);
    }

    [Fact]
    public void AliasTest()
    {
        Debugger debugger = new();
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "aa/default int8");
        Assert.Equal("!", logging.Encoded[0]);

        Decode(debugger, "w11a");
        Assert.Equal("!", logging.Encoded[1]);
        Assert.Equal(0x11, store.DefaultInt8);

        Decode(debugger, "aa/default int16");
        Assert.Equal("!", logging.Encoded[2]);
        Decode(debugger, "w12a");
        Assert.Equal("!", logging.Encoded[3]);
        Assert.Equal(0x11, store.DefaultInt8);
        Assert.Equal(0x12, store.DefaultInt16);

        Decode(debugger, "ra");
        Assert.Equal("12", logging.Encoded[4]);

        Decode(debugger, "aa");
        Assert.Equal("!", logging.Encoded[5]);

        Decode(debugger, "ra");
        Assert.Equal("?", logging.Encoded[6]);
    }

    [Fact]
    public void AliasBTest()
    {
        Debugger debugger = new();
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "ab/default int8");
        Assert.Equal("!", logging.Encoded[0]);

        Decode(debugger, "w11b");
        Assert.Equal("!", logging.Encoded[1]);
        Assert.Equal(0x11, store.DefaultInt8);
    }

    [Fact]
    public void MacroTest()
    {
        Debugger debugger = new();
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "m1;r/default uint8");
        Assert.Equal("!", logging.Encoded[0]);
        Decode(debugger, "1");
        Assert.Equal("0", logging.Encoded[1]);
        store.DefaultUint8 = 2;
        Decode(debugger, "1");
        Assert.Equal("2", logging.Encoded[2]);

        Decode(debugger, "m1|r/default uint8|e;|r/default uint16");
        Assert.Equal("!", logging.Encoded[3]);
        Decode(debugger, "1");
        Assert.Equal("2;0", logging.Encoded[4]);

        // Remove macro
        Decode(debugger, "m1");
        Assert.Equal("!", logging.Encoded[5]);
        Decode(debugger, "1");
        Assert.Equal("?", logging.Encoded[6]);

        // Recursive call
        Decode(debugger, "m1|e0|1|e0");
        Assert.Equal("!", logging.Encoded[7]);
        Decode(debugger, "1");
        Assert.Equal("0?0", logging.Encoded[8]);

        // Remove by itself is not allowed
        Decode(debugger, "m1|e0|m1|e0");
        Assert.Equal("!", logging.Encoded[9]);
        Decode(debugger, "1");
        Assert.Equal("0?0", logging.Encoded[10]);

        // Redefine by itself is not allowed
        Decode(debugger, "m1|e0|m1=?|e0");
        Assert.Equal("!", logging.Encoded[11]);
        Decode(debugger, "1");
        Assert.Equal("0?0", logging.Encoded[12]);

        // List macros
        Decode(debugger, "mMem");
        Assert.Equal("!", logging.Encoded[13]);
        Decode(debugger, "m");
        Assert.Equal("1M", logging.Encoded[14]);
    }

    [Fact]
    public void MacroMaxSizeTest()
    {
        Debugger debugger = new(maxMacrosSize: 16);
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "m1;e3456789abcdef");
        Assert.Equal("!", logging.Encoded[0]);
        Decode(debugger, "m3;e");
        Assert.Equal("?", logging.Encoded[1]);
    }

    [Fact]
    public void StreamTest()
    {
        Debugger debugger = new(maxStreamCount: 2);
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        // Use one stream, such that there is only one left.
        debugger.Stream('z', "oh gosh"u8);
        Decode(debugger, "s1");
        Assert.Equal("?", logging.Encoded[0]);

        debugger.Stream('1', "it's "u8);
        debugger.Stream('2', "a "u8); // no room for this stream
        debugger.Stream('1', "small "u8);

        debugger.Stream('1')!.Flush();
        Decode(debugger, "s1");
        Assert.Equal("it's small ", logging.Encoded[1]);
        Decode(debugger, "s1");
        Assert.Equal("", logging.Encoded[2]);

        debugger.Stream('1', "world "u8);
        debugger.Stream('1')!.Flush();
        Decode(debugger, "s1");
        Assert.Equal("world ", logging.Encoded[3]);

        debugger.Stream('3', "after "u8); // 1 is empty, so 3 can us it's space.
        debugger.Stream('3', "all "u8);
        debugger.Stream('3')!.Flush();
        debugger.Stream('1', "world "u8);
        Assert.Null(debugger.Stream('1'));

        Decode(debugger, "s3");
        Assert.Equal("after all ", logging.Encoded[4]);

        Decode(debugger, "s2");
        Assert.Equal("?", logging.Encoded[5]);
        Decode(debugger, "s1");
        Assert.Equal("?", logging.Encoded[6]);
    }

    [Fact]
    public void StreamOverflowTest()
    {
        Debugger debugger = new(maxStreamSize: 64);
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        // Use the Stream class' max size to create input larger than the stream can hold.
        int max = 64;
        int mod = 'z' - '0' + 1;
        byte[] payload = new byte[max + 8];
        for (int i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)('0' + i % mod);
        }

        // Append the data in a few chunks; the stream should only keep up to maxStreamSize bytes.
        debugger.Stream('1', payload.AsSpan(0, max / 2));
        debugger.Stream('1', payload.AsSpan(max / 2, max - (max / 2)));
        // This chunk should overflow and be discarded (or truncated) by the stream implementation.
        debugger.Stream('1', payload.AsSpan(max, payload.Length - max));

        // Flush and read the stream content.
        debugger.Stream('1')!.Flush();
        Decode(debugger, "s1");

        string expected = Encoding.UTF8.GetString(payload, 0, max);
        Assert.Single(logging.Encoded);
        Assert.Equal(expected, logging.Encoded[0]);
    }

    [Fact]
    public void TraceTest()
    {
        Debugger debugger = new();
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "mt|r/default uint8|e;");
        Assert.Equal("!", logging.Encoded[0]);

        Decode(debugger, "ttT");
        Assert.Equal("!", logging.Encoded[1]);

        debugger.Trace();
        // No compression, so no need to flush here
        Decode(debugger, "sT");
        Assert.Equal("0;", logging.Encoded[2]);

        store.DefaultUint8 = 1;
        debugger.Trace();
        store.DefaultUint8 = 2;
        debugger.Trace();

        Decode(debugger, "sT");
        Assert.Equal("1;2;", logging.Encoded[3]);

        // Set decimation
        Decode(debugger, "ttT3");
        Assert.Equal("!", logging.Encoded[4]);

        for (int i = 4; i < 10; i++)
        {
            store.DefaultUint8 = (byte)i;
            debugger.Trace();
        }

        Decode(debugger, "sT");
        Assert.Equal("6;9;", logging.Encoded[5]);

        // Disable
        Decode(debugger, "t");
        Assert.Equal("!", logging.Encoded[6]);

        debugger.Trace();
        debugger.Trace();
        debugger.Trace();

        Decode(debugger, "sT");
        Assert.Equal("", logging.Encoded[7]);
    }

    [Fact]
    public void ListReadTest()
    {
        Debugger debugger = new();
        TestStore store = new();
        debugger.Map(store);
        Protocol.LoggingLayer logging = new();
        logging.Wrap(debugger);

        Decode(debugger, "l");
        Assert.Single(logging.Encoded);
        string list = logging.Encoded[0];

        foreach (ReadOnlySpan<char> line in list.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            int pathIndex = line.IndexOf('/');
            ReadOnlySpan<char> typeStr = line.Slice(0, 2);
            ReadOnlySpan<char> sizeStr = line.Slice(2, pathIndex - 2);
            ReadOnlySpan<char> pathStr = line.Slice(pathIndex);

            Types type = TypesExtensions.Parse(typeStr);
            int size = int.Parse(sizeStr, NumberStyles.HexNumber);

            DebugVariant? debugVariant = debugger.Find(pathStr.ToString());

            // Make sure the metadata is correct
            Assert.NotNull(debugVariant);
            Assert.Equal(debugVariant.Type, type);
            Assert.Equal(debugVariant.Size, size);

            Decode(debugger, $"r{pathStr}");

            // Get the bytes.
            string hexValue = logging.Encoded[^1];
            string extendedHexValue = hexValue.PadLeft(size * 2, '0');
            byte[] bytes = Convert.FromHexString(extendedHexValue);

            // Ensure the bytes are in the endianness of this machine.
            byte[] dataBytes = BitConverter.IsLittleEndian && (type & Types.FlagFixed) != 0 ?
                bytes.ToArray().Reverse().ToArray() :
                bytes.ToArray();

            Assert.Equal(debugVariant.Get(), dataBytes);

            object value = TypesExtensions.ReadValue(bytes, type, size, bigEndian: true);
        }
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
