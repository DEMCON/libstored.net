namespace LibStored.Net.Generator.Models;

internal class TemplateObject
{
    public string Name { get; set; } = "";
    public string Hash { get; set; } = "";
    public string Init { get; set; } = "";
    public int Size { get; set; } = 0;
    public TemplateVariable[] Variables { get; set; } = [];
}
