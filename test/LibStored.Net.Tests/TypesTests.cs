// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Tests;

public class TypesTests
{
    [Fact]
    public void TestInt8Types()
    {
        TestStore store = new();
        Assert.Equal(0, store.DefaultInt8);
        store.DefaultInt8 = 42;
        Assert.Equal(42, store.DefaultInt8);
    }

    [Theory]
    [InlineData(0x1234)]
    [InlineData(-100)]
    public void TestInt16Types(short value)
    {
        TestStore store = new();
        Assert.Equal(0, store.DefaultInt16);
        store.DefaultInt16 = value;
        Assert.Equal(value, store.DefaultInt16);
    }

    [Theory]
    [InlineData(0x7abcdef0)]
    public void TestInt32Types(int value)
    {
        TestStore store = new();
        Assert.Equal(0, store.DefaultInt32);
        store.DefaultInt32 = value;
        Assert.Equal(value, store.DefaultInt32);
    }

    [Theory]
    [InlineData(0x0123456789abcdef)]
    public void TestInt64Types(long value)
    {
        TestStore store = new();
        Assert.Equal(0, store.DefaultInt64);
        store.DefaultInt64 = value;
        Assert.Equal(value, store.DefaultInt64);
    }

    [Theory]
    [InlineData(42)]
    public void TestUInt8Types(byte value)
    {
        TestStore store = new();
        Assert.Equal(0, store.DefaultUint8);
        store.DefaultUint8 = value;
        Assert.Equal(value, store.DefaultUint8);
    }

    [Theory]
    [InlineData(0x1234)]
    public void TestUInt16Types(ushort value)
    {
        TestStore store = new();
        Assert.Equal(0, store.DefaultUint16);
        store.DefaultUint16 = value;
        Assert.Equal(value, store.DefaultUint16);
    }

    [Theory]
    [InlineData(0x1234)]
    public void TestUInt32Types(uint value)
    {
        TestStore store = new();
        Assert.Equal(0u, store.DefaultUint32);
        store.DefaultUint32 = value;
        Assert.Equal(value, store.DefaultUint32);
    }

    [Theory]
    [InlineData(0xf123456789abcdef)]
    public void TestUInt64Types(ulong value)
    {
        TestStore store = new();
        Assert.Equal(0u, store.DefaultUint64);
        store.DefaultUint64 = value;
        Assert.Equal(value, store.DefaultUint64);
    }

    [Theory]
    [InlineData(3.14f)]
    public void TestFloatTypes(float value)
    {
        TestStore store = new();
        Assert.Equal(0u, store.DefaultFloat);
        store.DefaultFloat = value;
        Assert.Equal(value, store.DefaultFloat);
    }

    [Theory]
    [InlineData(3.14)]
    public void TestDoubleTypes(double value)
    {
        TestStore store = new();
        Assert.Equal(0u, store.DefaultDouble);
        store.DefaultDouble = value;
        Assert.Equal(value, store.DefaultDouble);
    }

    [Theory]
    [InlineData(true)]
    public void TestBoolTypes(bool value)
    {
        TestStore store = new();
        Assert.False(store.DefaultBool);
        store.DefaultBool = value;
        Assert.Equal(value, store.DefaultBool);
    }

    [Fact]
    public void TestBlobTypes()
    {
        TestStore store = new();
        int size = store.DefaultBlob.Length;
        byte[] buffer = new byte[size];
        byte[] values = Enumerable.Range(1, size).Select(x => (byte)x).ToArray();

        Assert.Equal(buffer, store.DefaultBlob);
        store.DefaultBlob = values;
        Assert.Equal(values, store.DefaultBlob);
    }

    [Fact]
    public void TestStringTypes()
    {
        TestStore store = new();
        string text = new('a', 10);

        Assert.Empty(store.DefaultString);
        store.DefaultString = text;
        Assert.Equal(text, store.DefaultString);
    }

    [Fact]
    public void TestStringThrowsTypes()
    {
        TestStore store = new();
        string text = new('a', 11);

        Assert.Empty(store.DefaultString);
        Assert.Throws<ArgumentException>(() => store.DefaultString = text);
    }

    [Theory]
    [InlineData(Types.Bool, 1, "01", true)]
    [InlineData(Types.Int8, 1, "12", (sbyte)18)]
    [InlineData(Types.Uint8, 1, "12", (byte)18)]
    [InlineData(Types.Int16, 2, "1234", (short)4_660)]
    [InlineData(Types.Uint16, 2, "1234", (ushort)4_660)]
    [InlineData(Types.Int32, 4, "12345678", (int)305_419_896)]
    [InlineData(Types.Uint32, 4, "12345678", (uint)305_419_896)]
    [InlineData(Types.Int64, 8, "1234567890abcdef", (long)1_311_768_467_294_899_695)]
    [InlineData(Types.Uint64, 8, "1234567890abcdef", (ulong)1_311_768_467_294_899_695)]
    [InlineData(Types.Float, 4, "4048F5C3", 3.14f)]
    [InlineData(Types.Double, 8, "40091EB851EB851F", 3.14)]
    [InlineData(Types.String, 6, "414243646566", "ABCdef")]
    [InlineData(Types.Blob, 6, "010203040506", new byte[]{ 1, 2, 3, 4, 5, 6})]
    public void ReadValueTest(Types type, int size, string hex, object expected)
    {
        byte[] bytes = Convert.FromHexString(hex);
        object value = TypesExtensions.ReadValue(bytes, type, size, bigEndian: true);
        Assert.Equal(expected, value);

        // Only also test little endian for numeric (fixed) types
        if (!type.IsFixed())
        {
            return;
        }

        byte[] bytesLittleEndian = bytes.Reverse().ToArray();
        object valueLittle = TypesExtensions.ReadValue(bytesLittleEndian, type, size, bigEndian: false);
        Assert.Equal(expected, valueLittle);
    }

    [Theory]
    [InlineData(Types.Bool, 1, "01", true)]
    [InlineData(Types.Int8, 1, "12", (sbyte)18)]
    [InlineData(Types.Uint8, 1, "12", (byte)18)]
    [InlineData(Types.Int16, 2, "1234", (short)4_660)]
    [InlineData(Types.Uint16, 2, "1234", (ushort)4_660)]
    [InlineData(Types.Int32, 4, "12345678", (int)305_419_896)]
    [InlineData(Types.Uint32, 4, "12345678", (uint)305_419_896)]
    [InlineData(Types.Int64, 8, "1234567890abcdef", (long)1_311_768_467_294_899_695)]
    [InlineData(Types.Uint64, 8, "1234567890abcdef", (ulong)1_311_768_467_294_899_695)]
    [InlineData(Types.Float, 4, "4048F5C3", 3.14f)]
    [InlineData(Types.Double, 8, "40091EB851EB851F", 3.14)]
    [InlineData(Types.String, 6, "414243646566", "ABCdef")]
    [InlineData(Types.Blob, 6, "010203040506", new byte[]{ 1, 2, 3, 4, 5, 6})]
    public void WriteValueTest(Types type, int size, string expected, object value)
    {
        byte[] buffer = new byte[size];
        TypesExtensions.WriteValue(value, buffer, type, size, bigEndian: true);
        string hex = Convert.ToHexString(buffer);
        Assert.Equal(expected, hex, StringComparer.OrdinalIgnoreCase);

        // Only also test little endian for numeric (fixed) types
        if (!type.IsFixed())
        {
            return;
        }

        byte[] bytesLittleEndian = new byte[size];
        TypesExtensions.WriteValue(value, bytesLittleEndian, type, size, bigEndian: false);
        byte[] bytesBigEndian = bytesLittleEndian.Reverse().ToArray();
        string hexBigEndian = Convert.ToHexString(bytesBigEndian);
        Assert.Equal(expected, hexBigEndian, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReadValueInvalidSizeTest()
    {
        string hex = "01";
        byte[] bytes = Convert.FromHexString(hex);
        Assert.Throws<ArgumentOutOfRangeException>(() => TypesExtensions.ReadValue(bytes, Types.Uint64, 8, bigEndian: true));
    }

    [Fact]
    public void ReadValueExtendBytesUint64Test()
    {
        const int size = sizeof(ulong);
        string hex = "01";
        string hexExtended = hex.PadLeft(size * 2, '0');
        byte[] bytes = Convert.FromHexString(hexExtended);
        object value = TypesExtensions.ReadValue(bytes, Types.Uint64, 8, bigEndian: true);
        Assert.Equal(1ul, value);
    }
}
