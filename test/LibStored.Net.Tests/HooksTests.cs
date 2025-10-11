// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.ComponentModel;
using LibStored.Net.Protocol;
using LibStored.Net.Synchronization;

namespace LibStored.Net.Tests;

public class HooksTests
{
    [Fact]
    public void ChangedTest()
    {
        TestStore store = new();
        List<PropertyChangedEventArgs> changed = [];
        store.PropertyChanged += (_, e) => changed.Add(e);

        Assert.Empty(changed);
        store.DefaultInt32 = 1;
        Assert.Single(changed);
    }

    [Fact]
    public void SynchronizableChangedTest()
    {
        SynchronizableStore<TestStore> store = new(new TestStore());
        List<PropertyChangedEventArgs> changed = [];
        store.Store().PropertyChanged += (_, e) => changed.Add(e);

        Assert.Empty(changed);
        store.Store().DefaultInt32 = 1;
        Assert.Single(changed);
    }

    [Fact]
    public void SyncTest()
    {
        SynchronizableStore<TestStore> store1 = new(new TestStore());
        SynchronizableStore<TestStore> store2 = new(new TestStore());
        int store1defaultInt32ChangeCount = 0;
        int store2defaultInt32ChangeCount = 0;
        store1.Store().PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TestStore.DefaultInt32))
            {
                store1defaultInt32ChangeCount++;
            }
        };
        store2.Store().PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TestStore.DefaultInt32))
            {
                store2defaultInt32ChangeCount++;
            }
        };

        Synchronizer s1 = new();
        Synchronizer s2 = new();

        ProtocolLayer p1 = new();
        ProtocolLayer p2 = new();
        LoopbackLayer loopbackLayer = new(p1, p2);

        s1.Map(store1);
        s2.Map(store2);
        s1.Connect(p1);
        s2.Connect(p2);

        s2.SyncFrom(store2, p2);

        Assert.Equal(0, store1defaultInt32ChangeCount);
        Assert.Equal(1, store2defaultInt32ChangeCount); // Because of the Welcome

        store1.Store().DefaultInt32 = 1;
        store1.Store().DefaultInt32 = 2;
        store1.Store().DefaultInt32 = 3;
        store1.Store().DefaultInt32 = 4;
        store1.Store().DefaultInt32 = 5;
        s1.Process();

        Assert.Equal(5, store1defaultInt32ChangeCount); // Local updates
        Assert.Equal(2, store2defaultInt32ChangeCount); // Because of Update
    }

    [Fact]
    public void ChangedWriteRecursionTest()
    {
        TestStore store = new();
        store.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(TestStore.DefaultInt32))
            {
                // Force a write during the change notification of another write
                store.DefaultInt16++;
            }
        };


        Assert.Equal(0, store.DefaultInt16);
        store.DefaultInt32 = 1;
        Assert.Equal(1, store.DefaultInt16);
    }
}
