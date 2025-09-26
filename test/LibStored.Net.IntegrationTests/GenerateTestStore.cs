// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Reflection;

namespace LibStored.Net.IntegrationTests;

public class GenerateTestStore
{
    [Fact]
    public void TestStoreCreated()
    {
        Type storeType = typeof(Store);
        // Use reflection
        List<Type> types = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => storeType.IsAssignableFrom(t)).ToList();

        Assert.Single(types);
        Assert.Equal("LibStored.Net.TestStore", types[0].FullName);
    }
}
