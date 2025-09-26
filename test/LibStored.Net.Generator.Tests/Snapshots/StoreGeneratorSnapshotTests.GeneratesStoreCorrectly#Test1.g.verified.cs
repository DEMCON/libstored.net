//HintName: Test1.g.cs
#nullable enable

namespace LibStored.Net;

/// <summary>
/// Test1 generated from TestStore.json.
/// </summary>
public class Test1 : global::LibStored.Net.Store, global::System.ComponentModel.INotifyPropertyChanged
{
    private static readonly byte[] InitialBuffer = [
        0x2A, 0x00, 0x00, 0x00
    ];

    private readonly byte[] _data = new byte[12];

    private readonly global::System.Collections.Generic.Dictionary<string, global::LibStored.Net.DebugVariantInfo> _debugDirectory = [];

    private readonly global::LibStored.Net.StoreVariable<int> _variable1;
    private readonly global::LibStored.Net.StoreVariable<double> _variable2;

    public Test1()
    {
        Test1.InitialBuffer.AsSpan().CopyTo(_data.AsSpan());

        _variable1 = new global::LibStored.Net.StoreVariable<int>(0, 4, this);
        _variable2 = new global::LibStored.Net.StoreVariable<double>(4, 8, this);

        _debugDirectory.Add("/Variable 1", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 0, 4));
        _debugDirectory.Add("/Variable 2", new global::LibStored.Net.DebugVariantInfo(Types.Double, 4, 8));
    }

    public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public override global::System.Span<byte> GetBuffer() => _data;
    public override global::System.Collections.Generic.IReadOnlyDictionary<string, global::LibStored.Net.DebugVariantInfo> GetDebugVariants() => _debugDirectory;

    public override string Name => "/Test1";
    public override string Hash => "1234";
    public override int VariableCount => 2;

    /// <summary>
    /// Variable 1.
    /// </summary>
    public int Variable1
    {
        get => _variable1.Get();
        set => _variable1.Set(value);
    }

    /// <summary>
    /// Variable 2.
    /// </summary>
    public double Variable2
    {
        get => _variable2.Get();
        set => _variable2.Set(value);
    }

    /// <summary>
    /// Notify property changed for the specific variable that changed based on the offset.
    /// </summary>
    /// <param name="offset"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public override void Changed(int offset)
    {
        if (PropertyChanged is null)
        {
            return;
        }

        global::System.ComponentModel.PropertyChangedEventArgs args = new(offset switch
        {
            0 => nameof(Test1.Variable1),
            4 => nameof(Test1.Variable2),
            _ => throw new ArgumentOutOfRangeException(nameof(offset), offset, "Unknown offset")
        });

        PropertyChanged?.Invoke(this, args);
    }
}
