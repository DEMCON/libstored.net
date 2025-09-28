using System.Text.Json.Serialization;

namespace LibStored.Net.Generator;

public class StoreModel
{
    [JsonRequired]
    public string Name { get; set; } = string.Empty;
    [JsonRequired]
    public string Hash { get; set; } = string.Empty;
    public Functions[] Functions { get; set; } = [];
    public Variables[] Variables { get; set; } = [];
}

public class Functions
{
    public string Name { get; set; } = string.Empty;
    public string Cname { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Ctype { get; set; } = string.Empty;
    public int Size { get; set; }
    public bool IsFunction { get; set; }
    public int? F { get; set; }
    public int? Offset { get; set; }
    [JsonIgnore]
    public byte[]? Init { get; set; }
    [JsonIgnore]
    public int? Axi { get; set; }
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
    public string Ctype { get; set; } = string.Empty;
    [JsonRequired]
    public int Size { get; set; }
    [JsonRequired]
    public int Offset { get; set; }
    public bool IsFunction { get; set; }
    public int? F { get; set; }
    public string? Init { get; set; }
    [JsonIgnore]
    public int? Axi { get; set; }
}

