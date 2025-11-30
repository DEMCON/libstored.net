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
            LittleEndian = true,
            Variables = [
                new Variables<int>
                {
                    Name = "Variable 1",
                    Cname = "variable_1",
                    Type = "int32",
                    Offset = 0,
                    Size = 4,
                    Init = 42
                },
                new Variables<double>
                {
                    Name = "Variable 2",
                    Cname = "variable_2",
                    Type = "double",
                    Offset = 4,
                    Size = 8
                }
            ]
        };
        string yaml = StoreYaml.Serializer.Serialize(model);
        await TestHelper.VerifyAdditionalText(yaml);
    }

    [Fact]
    public async Task GeneratesTestStoreCorrectly()
    {
        string json = await File.ReadAllTextAsync("TestStore.yml");
        await TestHelper.VerifyAdditionalText(json);
    }

    [Fact]
    public async Task GeneratesJsonDiagnosticsErrorMissingHash()
    {
        string yaml = """
                      name: "asd"
                      littleEndian: true
                      """;
        await TestHelper.VerifyAdditionalText(yaml);
    }

    [Fact]
    public async Task GeneratesJsonDiagnosticsInvalidInit()
    {
        string yaml = """
                      name: "asd"
                      hash: "qwe"
                      littleEndian: true
                      variables:
                        - name: "init var"
                          cname: "init_var"
                          type: "int32"
                          size: 8
                          offset: 0
                          init: "not a number"
                      """;
        await TestHelper.VerifyAdditionalText(yaml);
    }
}
