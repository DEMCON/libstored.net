
using System.Text.Json;

namespace LibStored.Net.Generator.Tests;

public class StoreGeneratorSnapshotTests
{
    [Fact]
    public async Task GeneratesStoreCorrectly()
    {
        StoreModel model = new()
        {
            Name = "Test1",
            Hash = "1234",
            Variables = [
                new Variables
                {
                    Name = "Variable 1",
                    Cname = "variable_1",
                    Type = "int32",
                    Offset = 0,
                    Size = 4,
                    Init = "KgAAAA=="
                },
                new Variables
                {
                    Name = "Variable 2",
                    Cname = "variable_2",
                    Type = "double",
                    Offset = 4,
                    Size = 8
                }
            ]
        };
        string json = JsonSerializer.Serialize(model);
        await TestHelper.VerifyAdditionalText(json);
    }
    
    [Fact]
    public async Task GeneratesTestStoreCorrectly()
    {
        string json = await File.ReadAllTextAsync("TestStore.json");
        await TestHelper.VerifyAdditionalText(json);
    }
    
    [Fact]
    public async Task GeneratesJsonDiagnosticsErrorMissingHash()
    {
        string json = """
                      {
                        "name": "asd", 
                      }
                      """;
        await TestHelper.VerifyAdditionalText(json);
    }
}