// // SPDX-FileCopyrightText: 2025 Guus Kuiper
// //
// // SPDX-License-Identifier: MIT

namespace LibStored.Net.Tests;

/// <summary>
/// Additional tests since a bool in c# uses 4 bytes, while libstored uses 1 byte.
/// </summary>
public class BoolTests
{
    [Fact]
    public void ReadBoolTest()
    {
        TestStore store = new TestStore();
        Variable<bool> b = store.GetVariable<bool>(270);
        Assert.False(b.Get());
    }
    
    [Fact]
    public void WriteBoolTest()
    {
        TestStore store = new TestStore();
        Variable<bool> b = store.GetVariable<bool>(270);
        b.Set(true);
        Assert.True(b.Get());
    }
    
    [Fact]
    public void WriteNearBoolTest()
    {
        // Arrange
        TestStore store = new TestStore();
        Variable<byte> before = store.GetVariable<byte>(269);
        Variable<bool> b = store.GetVariable<bool>(270);
        Variable<byte> after = store.GetVariable<byte>(271);
        before.Set(0xff);
        after.Set(0xff);
        b.Set(true);
        
        // Act
        b.Set(false);
        
        // Assert
        Assert.Equal(0xff, before.Get());
        Assert.Equal(0xff, after.Get());
    }
    
    [Fact]
    public void ReadBoolWriteNearTest()
    {
        // Arrange
        TestStore store = new TestStore();
        Variable<byte> before = store.GetVariable<byte>(269);
        Variable<bool> b = store.GetVariable<bool>(270);
        Variable<byte> after = store.GetVariable<byte>(271);
        b.Set(false);
        before.Set(0xff);
        after.Set(0xff);
        
        // Act
        bool value = b.Get();
        
        // Assert
        Assert.False(value);
    }
}