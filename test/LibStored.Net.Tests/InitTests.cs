// // SPDX-FileCopyrightText: 2025 Guus Kuiper
// //
// // SPDX-License-Identifier: MIT

namespace LibStored.Net.Tests;

public class InitTests
{
    [Fact]
    public void DecimalsTest()
    {
        TestStore store = new TestStore();
        Assert.Equal(42, store.InitDecimal);
        Assert.Equal(-42, store.InitNegative);
    }
    
    [Fact]
    public void HexTest()
    {
        TestStore store = new TestStore();
        Assert.Equal(0x54, store.InitHex);
    }
    
    [Fact]
    public void BinTest()
    {
        TestStore store = new TestStore();
        Assert.True(store.InitTrue);
        Assert.False(store.InitFalse);
        Assert.False(store.InitBool0);
        Assert.True(store.InitBool10);
    }

    [Fact]
    public void FloatTest()
    {
        TestStore store = new TestStore();
        Assert.Equal(0.0f, store.InitFloat0);
        Assert.Equal(1.0f, store.InitFloat1);
        Assert.Equal(3.14f, store.InitFloat314);
        Assert.Equal(-4000f, store.InitFloat4000);
        Assert.Equal(float.NaN, store.InitFloatNan);
        Assert.Equal(float.PositiveInfinity, store.InitFloatInf);
        Assert.Equal(float.NegativeInfinity, store.InitFloatNegInf);
    }

    [Fact]
    public void StringTest()
    {
        TestStore store = new TestStore();
        string buf = store.InitString;
        Assert.Equal("a b\"c", buf);
        
        Assert.Equal("", store.InitStringEmpty);
    }
}