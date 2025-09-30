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
        List<PropertyChangedEventArgs> changed1 = [];
        List<PropertyChangedEventArgs> changed2 = [];
        store1.Store().PropertyChanged += (_, e) => changed1.Add(e);
        store2.Store().PropertyChanged += (_, e) => changed2.Add(e);

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

        Assert.Empty(changed1);
        Assert.Single(changed2); // Because of the Welcome

        store1.Store().DefaultInt32 = 1;
        store1.Store().DefaultInt32 = 2;
        store1.Store().DefaultInt32 = 3;
        store1.Store().DefaultInt32 = 4;
        store1.Store().DefaultInt32 = 5;
        s1.Process();

        Assert.Equal(5, changed1.Count); // Local updates
        Assert.Equal(2, changed2.Count); // Because of Update
    }
}
