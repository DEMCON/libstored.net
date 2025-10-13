namespace LibStored.Net.Example;

/// <summary>
/// <code>
/// int32 number
/// double faction
/// string:15 text
/// </code>
/// </summary>
public class ExampleStore : Store
{
    private static readonly byte[] InitialBuffer = [0xFF, 0, 0, 0];

    private readonly byte[] _data = new byte[31];
    private readonly Dictionary<string, DebugVariantInfo> _debugDirectory = [];

    public ExampleStore()
    {
        ExampleStore.InitialBuffer.AsSpan().CopyTo(_data.AsSpan());

        Number = new StoreVariable<int>(0, 4, this);
        Fraction = new StoreVariable<double>(4, 8, this);
        Text = new StoreVariant<string>(12, 15, this);
        // How to align? Insert padding / sort?
        FourInts0 = new StoreVariable<sbyte>(27, 1, this);
        FourInts1 = new StoreVariable<sbyte>(28, 1, this);
        FourInts2 = new StoreVariable<sbyte>(29, 1, this);
        FourInts3 = new StoreVariable<sbyte>(30, 1, this);
        FourInts = [FourInts0, FourInts1, FourInts2, FourInts3];

        _debugDirectory.Add("/number", new DebugVariantInfo(Types.Int32, 0, 4));
        _debugDirectory.Add("/fraction", new DebugVariantInfo(Types.Double, 4, 8));
        _debugDirectory.Add("/text", new DebugVariantInfo(Types.String, 12, 15));
        _debugDirectory.Add("/four ints[0]", new DebugVariantInfo(Types.Int8, 27, 1));
        _debugDirectory.Add("/four ints[1]", new DebugVariantInfo(Types.Int8, 28, 1));
        _debugDirectory.Add("/four ints[3]", new DebugVariantInfo(Types.Int8, 29, 1));
        _debugDirectory.Add("/four ints[4]", new DebugVariantInfo(Types.Int8, 30, 1));
    }

    public override Span<byte> GetBuffer() => _data;

    /// <inheritdoc />
    public override void Changed(int offset) { }

    public override IReadOnlyDictionary<string, DebugVariantInfo> GetDebugVariants() => _debugDirectory;

    public override string Name => "ExampleStore";
    public override string Hash => "generated hash";
    public override int VariableCount => 7;

    public StoreVariable<int> Number { get; }
    public StoreVariable<double> Fraction { get; } 
    public StoreVariant<string> Text { get; }
    public StoreVariable<sbyte> FourInts0 { get; }
    public StoreVariable<sbyte> FourInts1 { get; }
    public StoreVariable<sbyte> FourInts2 { get; }
    public StoreVariable<sbyte> FourInts3 { get; }

    // How to deal with arrays? Libstored does not support it, just translate them to individual elements
    public StoreVariable<sbyte>[] FourInts { get; }

    public StoreVariable<sbyte> FoutInts(int i) => i switch
    {
        0 => FourInts0,
        1 => FourInts1,
        2 => FourInts2,
        3 => FourInts3,
        _ => throw new ArgumentOutOfRangeException(nameof(i), i, null)
    };
}
