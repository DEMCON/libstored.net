namespace LibStored.Net.Generator.Models;

internal class TemplateVariable
{
    public string Type { get; set; } = string.Empty;
    public string Cname { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Offset { get; set; }
    public int Size { get; set; }
    public byte[]? Value { get; set; }
}
