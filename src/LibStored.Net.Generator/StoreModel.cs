using System.Text.Json.Serialization;

namespace LibStored.Net.Generator;

public class StoreModel
{
    [JsonRequired]
    public string Name { get; set; } = string.Empty;
    [JsonRequired]
    public string Hash { get; set; } = string.Empty;
    [JsonRequired]
    public string Init { get; set; } = string.Empty;
    public Functions[] Functions { get; set; } = [];
    public Variables[] Variables { get; set; } = [];
}

public class Functions
{
    public string Name { get; set; } = string.Empty;
    public string Cname { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Size { get; set; }
    public int Function { get; set; }
}

public class Variables
{
    [JsonRequired]
    public string Name { get; set; } = string.Empty;
    [JsonRequired]
    public string Cname { get; set; } = string.Empty;
    [JsonRequired]
    public string Type { get; set; } = string.Empty;
    [JsonRequired]
    public int Size { get; set; }
    [JsonRequired]
    public int Offset { get; set; }
}

