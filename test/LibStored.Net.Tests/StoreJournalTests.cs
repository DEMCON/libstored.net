// // SPDX-FileCopyrightText: 2025 Guus Kuiper
// //
// // SPDX-License-Identifier: MIT

using LibStored.Net.Synchronization;

namespace LibStored.Net.Tests;

public class StoreJournalTests
{
    [Fact]
    public void HasChangedTest()
    {
        StoreJournal journal = new(new ExampleStore());
        const uint key = 1u;
        
        Assert.Equal(1u, journal.Seq);

        journal.Changed(key, 4);
        
        Assert.True(journal.HasChanged(key, 1));

        for (int i = 1; i < 50; i++)
        {
            journal.BumpSeq(true);
        }
        
        Assert.Equal(50u, journal.Seq);
        Assert.False(journal.HasChanged(key, 2));
    }
}