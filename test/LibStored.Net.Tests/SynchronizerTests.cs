// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Text;
using LibStored.Net.Debugging;
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
    public void SynchronizeInstantiateTest()
    {
        // Arrange
        SynchronizableStore<ExampleStore> store1 = new(new ExampleStore());
        SynchronizableStore<ExampleStore> store2 = new(new ExampleStore());
        
        AssertSyncStoresEqual(store1, store2);
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

    [Fact]
    public void Synchronize5Test()
    {
        var stores = new SynchronizableStore<TestStore>[5];
        var synchronizers = new Synchronizer[5];
        for (int i = 0; i < stores.Length; i++)
        {
            stores[i] = new SynchronizableStore<TestStore>(new TestStore());
            synchronizers[i] = new Synchronizer();
            synchronizers[i].Map(stores[i]);
        }
        
        /*
         * Topology: higher in tree is source.
         *
         *     0
         *    /  \
         *   1    2
         *       /  \
         *      3    4
         */

        Protocol.LoggingLayer logging01 = new();
        Protocol.LoggingLayer logging10 = new();
        Protocol.LoggingLayer logging02 = new();
        Protocol.LoggingLayer logging20 = new();
        Protocol.LoggingLayer logging23 = new();
        Protocol.LoggingLayer logging32 = new();
        Protocol.LoggingLayer logging24 = new();
        Protocol.LoggingLayer logging42 = new();
        
        Protocol.LoopbackLayer loop01 = new(logging01, logging10);
        Protocol.LoopbackLayer loop02 = new(logging02, logging20);
        Protocol.LoopbackLayer loop23 = new(logging23, logging32);
        Protocol.LoopbackLayer loop24 = new(logging24, logging42);

        synchronizers[0].Connect(logging01);
        synchronizers[0].Connect(logging02);
        synchronizers[1].Connect(logging10);
        synchronizers[2].Connect(logging20);
        synchronizers[2].Connect(logging23);
        synchronizers[2].Connect(logging24);
        synchronizers[3].Connect(logging32);
        synchronizers[4].Connect(logging42);

        synchronizers[1].SyncFrom(stores[1], logging10);
        synchronizers[2].SyncFrom(stores[2], logging20);
        synchronizers[3].SyncFrom(stores[3], logging32);
        synchronizers[4].SyncFrom(stores[4], logging42);

        for (int i = 1; i < 5; i++)
        {
            AssertSyncStoresEqual(stores[0], stores[i]);
        }
        
        stores[0].Store().DefaultUint8 = 1;
        AssertSyncStoresDifferent(stores[0], stores[1]);
        
        // Update stores connected to 0 (1 & 2)
        synchronizers[0].Process();
        AssertSyncStoresEqual(stores[0], stores[1]);
        Assert.Equal(0, stores[4].Store().DefaultUint8);
        
        // Update remaining stored connected to 2 (3 & 4)
        synchronizers[2].Process();
        
        Assert.Equal(1, stores[1].Store().DefaultUint8);
        Assert.Equal(1, stores[2].Store().DefaultUint8);
        Assert.Equal(1, stores[3].Store().DefaultUint8);
        Assert.Equal(1, stores[4].Store().DefaultUint8);
        
        for (int i = 1; i < 5; i++)
        {
            AssertSyncStoresEqual(stores[0], stores[i]);
        }
        
        stores[3].Store().DefaultInt16 = 2;
        stores[2].Store().DefaultInt32 = 3;
        stores[4].Store().DefaultUint8 = 4;
        stores[1].Store().DefaultUint16 = 5;
        stores[0].Store().DefaultUint32 = 6;

        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                synchronizers[i].Process();
            }
        }
        
        for (int i = 1; i < 5; i++)
        {
            AssertSyncStoresEqual(stores[0], stores[i]);
        }

        var lists = new List<DebugVariant>[5];
        for (int i = 0; i < 5; i++)
        {
            var list = new List<DebugVariant>();
            stores[i].Store().List( (_, d) =>
            {
                list.Add(d);
            });
            lists[i] = list;
        }

        int count = 0;
        int maxSize = lists[0].Max(x => x.Size);
        Span<byte> buffer = stackalloc byte[maxSize];
        long timestamp = Stopwatch.GetTimestamp();
        do
        {
            for (int batch = 0; batch < 10; batch++)
            {
                int i = Random.Shared.Next(0, 5);
                var list = lists[i];
                
                // Pick a random object from that store (but only one in five can be written
                // by us).
                DebugVariant o = list[(Random.Shared.Next() % (list.Count / 5)) * 5 + i];

                // Flip a bit of that object.
                Span<byte> data = buffer.Slice(0, o.Size);
                o.CopyTo(data);
                data[0] = (byte)(data[0] + 1);
                o.Set(data);
                count++;
            }
            
            // Full sync
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    synchronizers[i].Process();
                }
            }
            
            for (int i = 1; i < 5; i++)
            {
                AssertSyncStoresEqual(stores[0], stores[i]);
            }
        } while (Stopwatch.GetElapsedTime(timestamp).TotalSeconds < 1);
        
        _output.WriteLine($"Sync count {count}");
        Assert.True(count  > 100);
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
}