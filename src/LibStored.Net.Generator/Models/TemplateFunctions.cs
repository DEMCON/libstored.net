namespace LibStored.Net.Generator.Models;

public static class TemplateFunctions
{
    public static string CsTypes(string type) => type switch
    {
        "bool" => "bool",
        "int8" => "sbyte",
        "uint8" => "byte",
        "int16" => "short",
        "uint16" => "ushort",
        "int32" => "int",
        "uint32" => "uint",
        "int64" => "long",
        "uint64" => "ulong",
        "float" => "float",
        "double" => "double",
        "ptr32" => "uint",
        "ptr64" => "ulong",
        "blob" => "byte[]",
        "string" => "string",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
    
    public static string CseTypes(string type) => type switch
    {
        "bool" => "Types.Bool",
        "int8" => "Types.Int8",
        "uint8" => "Types.Uint8",
        "int16" => "Types.Int16",
        "uint16" => "Types.Uint16",
        "int32" => "Types.Int32",
        "uint32" => "Types.Uint32",
        "int64" => "Types.Int64",
        "uint64" => "Types.Uint64",
        "float" => "Types.Float",
        "double" => "Types.Double",
        "ptr32" => "Types.Pointer32",
        "ptr64" => "Types.Pointer64",
        "blob" => "Types.Blob",
        "string" => "Types.String",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
    
    public static string CsField(string type) 
    {
        string[] parts = type.Split('_');
        string prefix = parts.Length == 0 ? "_" : string.Empty;
        string field = $"{parts[0]}{string.Join("", parts.Skip(1).Select(ToUpperFirstLetter))}";
        return $"{prefix}_{field}";
    }
    
    public static string CsProp(string type)
    {
        string[] parts = type.Split('_');
        string prefix = parts.Length == 0 ? "_" : string.Empty;
        return $"{prefix}{string.Join("", parts.Select(ToUpperFirstLetter))}";
    }
    
    private static string ToUpperFirstLetter(string value) => value switch
    {
        "" => "",
        _ => char.ToUpper(value[0]) + value.Substring(1)
    };
} 