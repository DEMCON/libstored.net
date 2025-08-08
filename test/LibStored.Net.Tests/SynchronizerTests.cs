// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;
using LibStored.Net.Synchronization;
using Xunit.Abstractions;

namespace LibStored.Net.Tests;

public class SynchronizerTests
{
    private readonly ITestOutputHelper _output;

    public SynchronizerTests(ITestOutputHelper output)
    {
        _output = output;
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

    [Fact]
    public void SynchronizeInitialTest()
    {
        // Arrange
        SynchronizableStore<ExampleStore> store1 = new(new ExampleStore());
        SynchronizableStore<ExampleStore> store2 = new(new ExampleStore());

        Synchronizer s1 = new();
        Synchronizer s2 = new();

        Protocol.LoggingLayer logging1 = new();
        Protocol.LoggingLayer logging2 = new();

        Protocol.LoopbackLayer _ = new(logging1, logging2);

        s1.Map(store1);
        s2.Map(store2);
        s1.Connect(logging1);
        s2.Connect(logging2);

        AssertSyncStoresEqual(store1, store2);
        
        store1.Store().Number.Set(42);

        AssertSyncStoresDifferent(store1, store2);

        // Act
        s2.SyncFrom(store2, logging2);

        // Assert
        int number42 = store2.Store().Number.Get();
        
        Assert.Equal(42, number42);
        AssertSyncStoresEqual(store1, store2);

        foreach (string s in logging2.Encoded)
        {
            PrintBuffer(s, "> ");
        }
        foreach (string s in logging2.Decoded)
        {
            PrintBuffer(s, "< ");
        }
    }

    [Fact]
    public void ChangesTest()
    {
        SynchronizableStore<ExampleStore> store = new(new ExampleStore());

        ulong now = store.Journal().Seq;
        StoreVariable<int> n = store.Store().Number;

        store.Store().FourInts0.Variable().Key();
        
        uint keyN = n.Variable().Key();
        Assert.Empty(store.Journal().Changes());
        Assert.False(store.Journal().HasChanged(keyN, now));

        n.Set(1);
        Assert.True(store.Journal().HasChanged(keyN, now));
        Assert.Single(store.Journal().Changes());

        store.Store().Number.Set(2);
        Assert.True(store.Journal().HasChanged(keyN, now));
        Assert.False(store.Journal().HasChanged(keyN, now + 1));

        now = store.Journal().BumpSeq();
        Assert.False(store.Journal().HasChanged(keyN, now));

        DebugVariant f = store.Store().Find("/fraction")!;
        uint keyF = f.Variant().Key();
        Assert.False(store.Journal().HasChanged(keyF, now));

        f.Set(BitConverter.GetBytes(3.14d));
        Assert.True(store.Journal().HasChanged(keyF, now));

        Assert.False(store.Journal().HasChanged(keyN, now));
        Assert.True(store.Journal().HasChanged(now));

        Assert.Equal(2, store.Journal().Changes().Count());
    }

    [Fact]
    public void SynchronizeTest()
    {
        SynchronizableStore<ExampleStore> store1 = new(new ExampleStore());
        SynchronizableStore<ExampleStore> store2 = new(new ExampleStore());

        Synchronizer s1 = new();
        Synchronizer s2 = new();

        Protocol.LoggingLayer logging1 = new();
        Protocol.LoggingLayer logging2 = new();

        Protocol.LoopbackLayer _ = new(logging1, logging2);

        s1.Map(store1);
        s2.Map(store2);
        s1.Connect(logging1);
        s2.Connect(logging2);

        // Equal at initialization.
        AssertSyncStoresEqual(store1, store2);

        s2.SyncFrom(store2, logging2);

        store1.Store().Number.Set(2);
        store1.Store().Fraction.Set(3.14);
        store1.Store().Text.Set("Hello World!"u8);

        // Not synced yet.
        AssertSyncStoresDifferent(store1, store2);

        s1.Process();

        // Equal after sync.
        int number = store2.Store().Number.Get();
        Assert.Equal(2, number);
        Assert.Equal(3.14, store2.Store().Fraction.Get());
        Assert.Equal("Hello World!"u8, store2.Store().Text.Get().TrimEnd((byte)0));
        AssertSyncStoresEqual(store1, store2);

        // Change from the other side.
        store2.Store().Number.Set(3);

        AssertSyncStoresDifferent(store1, store2);

        s2.Process();

        // Equal after sync.
        AssertSyncStoresEqual(store1, store2);

        // Change from both sides.
        store1.Store().Number.Set(4);
        store2.Store().Fraction.Set(6.28);
        AssertSyncStoresDifferent(store1, store2);

        s1.Process();
        s2.Process();

        // Equal after sync.
        AssertSyncStoresEqual(store1, store2);

        foreach (string s in logging2.Encoded)
        {
            PrintBuffer(s, "> ");
        }
        foreach (string s in logging2.Decoded)
        {
            PrintBuffer(s, "< ");
        }
    }

    private void PrintBuffer(string text, string prefix = "")
    {
        string result = StringUtils.StringLiteral(text, prefix);
        _output.WriteLine(result);
    }

    private void AssertSyncStoresEqual<TStore>(SynchronizableStore<TStore> a, SynchronizableStore<TStore> b) where TStore : Store => AssertDebugStoresEqual(a.Store(), b.Store());
    private void AssertDebugStoresEqual(Store a, Store b)
    {
        Assert.Equal(a.Hash, b.Hash);

        a.List((path, va) =>
        {
            DebugVariant? vb = b.Find(path);
            Assert.NotNull(vb);
            Assert.Equivalent(va, vb);
            Assert.Equal(va.Get(), vb.Get());
        });
    }

    private void AssertSyncStoresDifferent<TStore>(SynchronizableStore<TStore> a, SynchronizableStore<TStore> b) where TStore : Store => AssertDebugStoresDifferent(a.Store(), b.Store());
    private void AssertDebugStoresDifferent(Store a, Store b)
    {
        Assert.Equal(a.Hash, b.Hash);

        List<(DebugVariant v1, DebugVariant v2)> couples = [];

        a.List((path, va) =>
        {
            DebugVariant? vb = b.Find(path);
            Assert.NotNull(vb);

            couples.Add((va, vb));
        });

        bool equal = couples.All(tuple => SynchronizerTests.Equal(tuple.v1, tuple.v2));
        Assert.False(equal);
    }

    private static bool Equal(DebugVariant a, DebugVariant b) => a.Offset == b.Offset && a.Size == b.Size && a.Type == b.Type && a.Get().SequenceEqual(b.Get());

    private void Encode(Protocol.ProtocolLayer layer, string data, bool last = true)
    {
        byte[] bytes = SynchronizerTests.Bytes(data);
        layer.Encode(bytes, last);
    }

    private void Decode(Protocol.ProtocolLayer layer, string data)
    {
        byte[] bytes = SynchronizerTests.Bytes(data);
        layer.Decode(bytes);
    }

    private static byte[] Bytes(string data) => Encoding.ASCII.GetBytes(data);
    private static string String(byte[] data) => Encoding.ASCII.GetString(data);
}