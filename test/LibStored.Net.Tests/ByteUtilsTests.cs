// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Tests;

public class ByteUtilsTests
{
    [Fact]
    public void WriteBigEndianUin8Test()
    {
        byte[] buffer = new byte[1];
        ByteUtils.WriteUInt8(buffer, 0x1, true);
        byte swapped = ByteUtils.ReadUInt8(buffer, false);
        Assert.Equal(0x1, swapped);
    }

    [Fact]
    public void WriteBigEndianUin16Test()
    {
        byte[] buffer = new byte[2];
        ByteUtils.WriteUInt16(buffer, 0x1234, true);
        ushort swapped = ByteUtils.ReadUInt16(buffer, false);
        Assert.Equal(0x3412u, swapped);
    }

    [Fact]
    public void WriteBigEndianUin32Test()
    {
        byte[] buffer = new byte[4];
        ByteUtils.WriteUInt32(buffer, 0x12345678, true);
        uint swapped = ByteUtils.ReadUInt32(buffer, false);
        Assert.Equal(0x78563412u, swapped);
    }
}
