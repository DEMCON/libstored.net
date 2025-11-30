namespace LibStored.Net.Generator;

internal class StoreModel
{
    public string Name { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;

    public bool LittleEndian { get; set; } = false;
    public Functions[] Functions { get; set; } = [];
    public Variables[] Variables { get; set; } = [];
}

internal class Functions
{
    public string Name { get; set; } = string.Empty;
    public string Cname { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Size { get; set; }
    public int Function { get; set; }
}

internal class Variables
{
    public string Name { get; set; } = string.Empty;
    public string Cname { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Size { get; set; }
    public int Offset { get; set; }

    public virtual byte[]? ToBytes() => null;
}

internal class VariablesString : Variables
{
    public string? Init { get; set; }
    public override byte[]? ToBytes() => Init is null ? null : System.Text.Encoding.UTF8.GetBytes(Init);
}

internal class Variables<T> : Variables
    where T : struct
{
    public T? Init { get; set; }

    public override byte[]? ToBytes() => Init switch
    {
        null => null,
        int v when typeof(T) == typeof(int) && Size == 4 => BitConverter.GetBytes(v),
        uint v when typeof(T) == typeof(uint) && Size == 4 => BitConverter.GetBytes(v),
        short v when typeof(T) == typeof(short) && Size == 2 => BitConverter.GetBytes(v),
        ushort v when typeof(T) == typeof(ushort) && Size == 2 => BitConverter.GetBytes(v),
        long v when typeof(T) == typeof(long) && Size == 8 => BitConverter.GetBytes(v),
        ulong v when typeof(T) == typeof(ulong) && Size == 8 => BitConverter.GetBytes(v),
        float v when typeof(T) == typeof(float) && Size == 4 => BitConverter.GetBytes(v),
        double v when typeof(T) == typeof(double) && Size == 8 => BitConverter.GetBytes(v),
        bool v when typeof(T) == typeof(bool) && Size == 1 => [v ? (byte)1 : (byte)0],
        _ => null
    };
}
