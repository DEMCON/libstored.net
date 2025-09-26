//HintName: TestStore.g.cs
#nullable enable

namespace LibStored.Net;

/// <summary>
/// TestStore generated from TestStore.json.
/// </summary>
public class TestStore : global::LibStored.Net.Store, global::System.ComponentModel.INotifyPropertyChanged
{
    private static readonly byte[] InitialBuffer = [
        0x61, 0x20, 0x62, 0x22, 0x63, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0xC0, 0x2A, 0x00, 0x00, 0x00, 0xD6, 0xFF, 0xFF, 0xFF, 0x54, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3F, 0xC3, 0xF5, 0x48, 0x40, 0x00, 0x00, 0x7A, 0xC5, 0x00, 0x00, 0xC0, 0x7F, 0x00, 0x00, 0x80, 0x7F, 0x00, 0x00, 0x80, 0xFF, 0x00, 0x00, 0x40, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x3F, 0x00, 0x00, 0x80, 0xBF, 0x00, 0x00, 0x20, 0x41, 0x00, 0x00, 0xC0, 0x7F, 0x00, 0x00, 0x60, 0x40, 0x00, 0x00, 0xC0, 0x7F, 0x00, 0x00, 0x80, 0xBF, 0x01, 0x01, 0x01, 0x01, 0x01
    ];

    private readonly byte[] _data = new byte[277];

    private readonly global::System.Collections.Generic.Dictionary<string, global::LibStored.Net.DebugVariantInfo> _debugDirectory = [];

    private readonly global::LibStored.Net.StoreVariant<string> _initString;
    private readonly global::LibStored.Net.StoreVariant<double> _doubleAmpGain;
    private readonly global::LibStored.Net.StoreVariant<int> _initDecimal;
    private readonly global::LibStored.Net.StoreVariant<int> _initNegative;
    private readonly global::LibStored.Net.StoreVariant<int> _initHex;
    private readonly global::LibStored.Net.StoreVariant<int> _initBin;
    private readonly global::LibStored.Net.StoreVariant<float> _initFloat1;
    private readonly global::LibStored.Net.StoreVariant<float> _initFloat314;
    private readonly global::LibStored.Net.StoreVariant<float> _initFloat4000;
    private readonly global::LibStored.Net.StoreVariant<float> _initFloatNan;
    private readonly global::LibStored.Net.StoreVariant<float> _initFloatInf;
    private readonly global::LibStored.Net.StoreVariant<float> _initFloatNegInf;
    private readonly global::LibStored.Net.StoreVariant<float> _arraySingle;
    private readonly global::LibStored.Net.StoreVariant<float> _ampGain;
    private readonly global::LibStored.Net.StoreVariant<float> _ampOffset;
    private readonly global::LibStored.Net.StoreVariant<float> _ampLow;
    private readonly global::LibStored.Net.StoreVariant<float> _ampHigh;
    private readonly global::LibStored.Net.StoreVariant<float> _ampOverride;
    private readonly global::LibStored.Net.StoreVariant<float> _smallAmpGain;
    private readonly global::LibStored.Net.StoreVariant<float> _smallAmpOverride;
    private readonly global::LibStored.Net.StoreVariant<float> _ambiguousAmpGain;
    private readonly global::LibStored.Net.StoreVariant<bool> _initTrue;
    private readonly global::LibStored.Net.StoreVariant<bool> _initBool10;
    private readonly global::LibStored.Net.StoreVariant<bool> _arrayBool0;
    private readonly global::LibStored.Net.StoreVariant<bool> _arrayBool1;
    private readonly global::LibStored.Net.StoreVariant<bool> _ampEnable;
    private readonly global::LibStored.Net.StoreVariant<string> _defaultString;
    private readonly global::LibStored.Net.StoreVariant<long> _defaultInt64;
    private readonly global::LibStored.Net.StoreVariant<ulong> _defaultUint64;
    private readonly global::LibStored.Net.StoreVariant<double> _defaultDouble;
    private readonly global::LibStored.Net.StoreVariant<ulong> _defaultPtr64;
    private readonly global::LibStored.Net.StoreVariant<string> _initStringEmpty;
    private readonly global::LibStored.Net.StoreVariant<byte[]> _defaultBlob;
    private readonly global::LibStored.Net.StoreVariant<int> _defaultInt32;
    private readonly global::LibStored.Net.StoreVariant<uint> _defaultUint32;
    private readonly global::LibStored.Net.StoreVariant<float> _defaultFloat;
    private readonly global::LibStored.Net.StoreVariant<uint> _defaultPtr32;
    private readonly global::LibStored.Net.StoreVariant<float> _initFloat0;
    private readonly global::LibStored.Net.StoreVariant<string> _arrayString0;
    private readonly global::LibStored.Net.StoreVariant<string> _arrayString1;
    private readonly global::LibStored.Net.StoreVariant<string> _arrayString2;
    private readonly global::LibStored.Net.StoreVariant<int> _scopeInnerInt;
    private readonly global::LibStored.Net.StoreVariant<float> _valueWithUnitKmS;
    private readonly global::LibStored.Net.StoreVariant<float> _valueWithComplexUnitJSM2;
    private readonly global::LibStored.Net.StoreVariant<float> _valueWithAbiguousUnitMS;
    private readonly global::LibStored.Net.StoreVariant<float> _valueWithAbiguousUnitMH;
    private readonly global::LibStored.Net.StoreVariant<float> _ampInput;
    private readonly global::LibStored.Net.StoreVariant<float> _ampOutput;
    private readonly global::LibStored.Net.StoreVariant<float> _smallAmpOutput;
    private readonly global::LibStored.Net.StoreVariant<float> _ambiguousAmpOutput;
    private readonly global::LibStored.Net.StoreVariant<short> _defaultInt16;
    private readonly global::LibStored.Net.StoreVariant<ushort> _defaultUint16;
    private readonly global::LibStored.Net.StoreVariant<sbyte> _defaultInt8;
    private readonly global::LibStored.Net.StoreVariant<byte> _defaultUint8;
    private readonly global::LibStored.Net.StoreVariant<bool> _defaultBool;
    private readonly global::LibStored.Net.StoreVariant<bool> _initFalse;
    private readonly global::LibStored.Net.StoreVariant<bool> _initBool0;
    private readonly global::LibStored.Net.StoreVariant<bool> _arrayBool2;
    private readonly global::LibStored.Net.StoreVariant<bool> _scopeInnerBool;
    private readonly global::LibStored.Net.StoreVariant<bool> _someOtherScopeSomeOtherInnerBool;
    private readonly global::LibStored.Net.StoreVariant<bool> _ambiguousAmpEnable;

    public TestStore()
    {
        TestStore.InitialBuffer.AsSpan().CopyTo(_data.AsSpan());

        _initString = new global::LibStored.Net.StoreVariant<string>(0, 8, this);
        _doubleAmpGain = new global::LibStored.Net.StoreVariant<double>(16, 8, this);
        _initDecimal = new global::LibStored.Net.StoreVariant<int>(24, 4, this);
        _initNegative = new global::LibStored.Net.StoreVariant<int>(28, 4, this);
        _initHex = new global::LibStored.Net.StoreVariant<int>(32, 4, this);
        _initBin = new global::LibStored.Net.StoreVariant<int>(36, 4, this);
        _initFloat1 = new global::LibStored.Net.StoreVariant<float>(40, 4, this);
        _initFloat314 = new global::LibStored.Net.StoreVariant<float>(44, 4, this);
        _initFloat4000 = new global::LibStored.Net.StoreVariant<float>(48, 4, this);
        _initFloatNan = new global::LibStored.Net.StoreVariant<float>(52, 4, this);
        _initFloatInf = new global::LibStored.Net.StoreVariant<float>(56, 4, this);
        _initFloatNegInf = new global::LibStored.Net.StoreVariant<float>(60, 4, this);
        _arraySingle = new global::LibStored.Net.StoreVariant<float>(64, 4, this);
        _ampGain = new global::LibStored.Net.StoreVariant<float>(68, 4, this);
        _ampOffset = new global::LibStored.Net.StoreVariant<float>(72, 4, this);
        _ampLow = new global::LibStored.Net.StoreVariant<float>(76, 4, this);
        _ampHigh = new global::LibStored.Net.StoreVariant<float>(80, 4, this);
        _ampOverride = new global::LibStored.Net.StoreVariant<float>(84, 4, this);
        _smallAmpGain = new global::LibStored.Net.StoreVariant<float>(88, 4, this);
        _smallAmpOverride = new global::LibStored.Net.StoreVariant<float>(92, 4, this);
        _ambiguousAmpGain = new global::LibStored.Net.StoreVariant<float>(96, 4, this);
        _initTrue = new global::LibStored.Net.StoreVariant<bool>(100, 1, this);
        _initBool10 = new global::LibStored.Net.StoreVariant<bool>(101, 1, this);
        _arrayBool0 = new global::LibStored.Net.StoreVariant<bool>(102, 1, this);
        _arrayBool1 = new global::LibStored.Net.StoreVariant<bool>(103, 1, this);
        _ampEnable = new global::LibStored.Net.StoreVariant<bool>(104, 1, this);
        _defaultString = new global::LibStored.Net.StoreVariant<string>(112, 10, this);
        _defaultInt64 = new global::LibStored.Net.StoreVariant<long>(128, 8, this);
        _defaultUint64 = new global::LibStored.Net.StoreVariant<ulong>(136, 8, this);
        _defaultDouble = new global::LibStored.Net.StoreVariant<double>(144, 8, this);
        _defaultPtr64 = new global::LibStored.Net.StoreVariant<ulong>(152, 8, this);
        _initStringEmpty = new global::LibStored.Net.StoreVariant<string>(160, 8, this);
        _defaultBlob = new global::LibStored.Net.StoreVariant<byte[]>(176, 5, this);
        _defaultInt32 = new global::LibStored.Net.StoreVariant<int>(184, 4, this);
        _defaultUint32 = new global::LibStored.Net.StoreVariant<uint>(188, 4, this);
        _defaultFloat = new global::LibStored.Net.StoreVariant<float>(192, 4, this);
        _defaultPtr32 = new global::LibStored.Net.StoreVariant<uint>(196, 4, this);
        _initFloat0 = new global::LibStored.Net.StoreVariant<float>(200, 4, this);
        _arrayString0 = new global::LibStored.Net.StoreVariant<string>(204, 4, this);
        _arrayString1 = new global::LibStored.Net.StoreVariant<string>(212, 4, this);
        _arrayString2 = new global::LibStored.Net.StoreVariant<string>(220, 4, this);
        _scopeInnerInt = new global::LibStored.Net.StoreVariant<int>(228, 4, this);
        _valueWithUnitKmS = new global::LibStored.Net.StoreVariant<float>(232, 4, this);
        _valueWithComplexUnitJSM2 = new global::LibStored.Net.StoreVariant<float>(236, 4, this);
        _valueWithAbiguousUnitMS = new global::LibStored.Net.StoreVariant<float>(240, 4, this);
        _valueWithAbiguousUnitMH = new global::LibStored.Net.StoreVariant<float>(244, 4, this);
        _ampInput = new global::LibStored.Net.StoreVariant<float>(248, 4, this);
        _ampOutput = new global::LibStored.Net.StoreVariant<float>(252, 4, this);
        _smallAmpOutput = new global::LibStored.Net.StoreVariant<float>(256, 4, this);
        _ambiguousAmpOutput = new global::LibStored.Net.StoreVariant<float>(260, 4, this);
        _defaultInt16 = new global::LibStored.Net.StoreVariant<short>(264, 2, this);
        _defaultUint16 = new global::LibStored.Net.StoreVariant<ushort>(266, 2, this);
        _defaultInt8 = new global::LibStored.Net.StoreVariant<sbyte>(268, 1, this);
        _defaultUint8 = new global::LibStored.Net.StoreVariant<byte>(269, 1, this);
        _defaultBool = new global::LibStored.Net.StoreVariant<bool>(270, 1, this);
        _initFalse = new global::LibStored.Net.StoreVariant<bool>(271, 1, this);
        _initBool0 = new global::LibStored.Net.StoreVariant<bool>(272, 1, this);
        _arrayBool2 = new global::LibStored.Net.StoreVariant<bool>(273, 1, this);
        _scopeInnerBool = new global::LibStored.Net.StoreVariant<bool>(274, 1, this);
        _someOtherScopeSomeOtherInnerBool = new global::LibStored.Net.StoreVariant<bool>(275, 1, this);
        _ambiguousAmpEnable = new global::LibStored.Net.StoreVariant<bool>(276, 1, this);

        _debugDirectory.Add("/init string", new global::LibStored.Net.DebugVariantInfo(Types.String, 0, 8));
        _debugDirectory.Add("/double amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Double, 16, 8));
        _debugDirectory.Add("/init decimal", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 24, 4));
        _debugDirectory.Add("/init negative", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 28, 4));
        _debugDirectory.Add("/init hex", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 32, 4));
        _debugDirectory.Add("/init bin", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 36, 4));
        _debugDirectory.Add("/init float 1", new global::LibStored.Net.DebugVariantInfo(Types.Float, 40, 4));
        _debugDirectory.Add("/init float 3.14", new global::LibStored.Net.DebugVariantInfo(Types.Float, 44, 4));
        _debugDirectory.Add("/init float -4000", new global::LibStored.Net.DebugVariantInfo(Types.Float, 48, 4));
        _debugDirectory.Add("/init float nan", new global::LibStored.Net.DebugVariantInfo(Types.Float, 52, 4));
        _debugDirectory.Add("/init float inf", new global::LibStored.Net.DebugVariantInfo(Types.Float, 56, 4));
        _debugDirectory.Add("/init float neg inf", new global::LibStored.Net.DebugVariantInfo(Types.Float, 60, 4));
        _debugDirectory.Add("/array single", new global::LibStored.Net.DebugVariantInfo(Types.Float, 64, 4));
        _debugDirectory.Add("/amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Float, 68, 4));
        _debugDirectory.Add("/amp/offset", new global::LibStored.Net.DebugVariantInfo(Types.Float, 72, 4));
        _debugDirectory.Add("/amp/low", new global::LibStored.Net.DebugVariantInfo(Types.Float, 76, 4));
        _debugDirectory.Add("/amp/high", new global::LibStored.Net.DebugVariantInfo(Types.Float, 80, 4));
        _debugDirectory.Add("/amp/override", new global::LibStored.Net.DebugVariantInfo(Types.Float, 84, 4));
        _debugDirectory.Add("/small amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Float, 88, 4));
        _debugDirectory.Add("/small amp/override", new global::LibStored.Net.DebugVariantInfo(Types.Float, 92, 4));
        _debugDirectory.Add("/ambiguous amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Float, 96, 4));
        _debugDirectory.Add("/init true", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 100, 1));
        _debugDirectory.Add("/init bool 10", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 101, 1));
        _debugDirectory.Add("/array bool[0]", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 102, 1));
        _debugDirectory.Add("/array bool[1]", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 103, 1));
        _debugDirectory.Add("/amp/enable", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 104, 1));
        _debugDirectory.Add("/default string", new global::LibStored.Net.DebugVariantInfo(Types.String, 112, 10));
        _debugDirectory.Add("/default int64", new global::LibStored.Net.DebugVariantInfo(Types.Int64, 128, 8));
        _debugDirectory.Add("/default uint64", new global::LibStored.Net.DebugVariantInfo(Types.Uint64, 136, 8));
        _debugDirectory.Add("/default double", new global::LibStored.Net.DebugVariantInfo(Types.Double, 144, 8));
        _debugDirectory.Add("/default ptr64", new global::LibStored.Net.DebugVariantInfo(Types.Pointer64, 152, 8));
        _debugDirectory.Add("/init string empty", new global::LibStored.Net.DebugVariantInfo(Types.String, 160, 8));
        _debugDirectory.Add("/default blob", new global::LibStored.Net.DebugVariantInfo(Types.Blob, 176, 5));
        _debugDirectory.Add("/default int32", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 184, 4));
        _debugDirectory.Add("/default uint32", new global::LibStored.Net.DebugVariantInfo(Types.Uint32, 188, 4));
        _debugDirectory.Add("/default float", new global::LibStored.Net.DebugVariantInfo(Types.Float, 192, 4));
        _debugDirectory.Add("/default ptr32", new global::LibStored.Net.DebugVariantInfo(Types.Pointer32, 196, 4));
        _debugDirectory.Add("/init float 0", new global::LibStored.Net.DebugVariantInfo(Types.Float, 200, 4));
        _debugDirectory.Add("/array string[0]", new global::LibStored.Net.DebugVariantInfo(Types.String, 204, 4));
        _debugDirectory.Add("/array string[1]", new global::LibStored.Net.DebugVariantInfo(Types.String, 212, 4));
        _debugDirectory.Add("/array string[2]", new global::LibStored.Net.DebugVariantInfo(Types.String, 220, 4));
        _debugDirectory.Add("/scope/inner int", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 228, 4));
        _debugDirectory.Add("/value with unit (km/s)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 232, 4));
        _debugDirectory.Add("/value with complex unit (J/s/m^2)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 236, 4));
        _debugDirectory.Add("/value with abiguous unit (m/s)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 240, 4));
        _debugDirectory.Add("/value with abiguous unit (m/h)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 244, 4));
        _debugDirectory.Add("/amp/input", new global::LibStored.Net.DebugVariantInfo(Types.Float, 248, 4));
        _debugDirectory.Add("/amp/output", new global::LibStored.Net.DebugVariantInfo(Types.Float, 252, 4));
        _debugDirectory.Add("/small amp/output", new global::LibStored.Net.DebugVariantInfo(Types.Float, 256, 4));
        _debugDirectory.Add("/ambiguous amp/output", new global::LibStored.Net.DebugVariantInfo(Types.Float, 260, 4));
        _debugDirectory.Add("/default int16", new global::LibStored.Net.DebugVariantInfo(Types.Int16, 264, 2));
        _debugDirectory.Add("/default uint16", new global::LibStored.Net.DebugVariantInfo(Types.Uint16, 266, 2));
        _debugDirectory.Add("/default int8", new global::LibStored.Net.DebugVariantInfo(Types.Int8, 268, 1));
        _debugDirectory.Add("/default uint8", new global::LibStored.Net.DebugVariantInfo(Types.Uint8, 269, 1));
        _debugDirectory.Add("/default bool", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 270, 1));
        _debugDirectory.Add("/init false", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 271, 1));
        _debugDirectory.Add("/init bool 0", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 272, 1));
        _debugDirectory.Add("/array bool[2]", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 273, 1));
        _debugDirectory.Add("/scope/inner bool", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 274, 1));
        _debugDirectory.Add("/some other scope/some other inner bool", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 275, 1));
        _debugDirectory.Add("/ambiguous amp/enable", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 276, 1));
    }

    public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public override global::System.Span<byte> GetBuffer() => _data;
    public override global::System.Collections.Generic.IReadOnlyDictionary<string, global::LibStored.Net.DebugVariantInfo> GetDebugVariants() => _debugDirectory;

    public override string Name => "/TestStore";
    public override string Hash => "b0bba06dabc9dff2a4c18c4ebdcc39e1418575b4";
    public override int VariableCount => 61;

    /// <summary>
    /// init string.
    /// </summary>
    public string InitString
    {
        get => StoreVariantExtensions.Get(_initString);
        set => StoreVariantExtensions.Set(_initString, value);
    }

    /// <summary>
    /// double amp/gain.
    /// </summary>
    public double DoubleAmpGain
    {
        get => StoreVariantExtensions.Get(_doubleAmpGain);
        set => StoreVariantExtensions.Set(_doubleAmpGain, value);
    }

    /// <summary>
    /// init decimal.
    /// </summary>
    public int InitDecimal
    {
        get => StoreVariantExtensions.Get(_initDecimal);
        set => StoreVariantExtensions.Set(_initDecimal, value);
    }

    /// <summary>
    /// init negative.
    /// </summary>
    public int InitNegative
    {
        get => StoreVariantExtensions.Get(_initNegative);
        set => StoreVariantExtensions.Set(_initNegative, value);
    }

    /// <summary>
    /// init hex.
    /// </summary>
    public int InitHex
    {
        get => StoreVariantExtensions.Get(_initHex);
        set => StoreVariantExtensions.Set(_initHex, value);
    }

    /// <summary>
    /// init bin.
    /// </summary>
    public int InitBin
    {
        get => StoreVariantExtensions.Get(_initBin);
        set => StoreVariantExtensions.Set(_initBin, value);
    }

    /// <summary>
    /// init float 1.
    /// </summary>
    public float InitFloat1
    {
        get => StoreVariantExtensions.Get(_initFloat1);
        set => StoreVariantExtensions.Set(_initFloat1, value);
    }

    /// <summary>
    /// init float 3.14.
    /// </summary>
    public float InitFloat314
    {
        get => StoreVariantExtensions.Get(_initFloat314);
        set => StoreVariantExtensions.Set(_initFloat314, value);
    }

    /// <summary>
    /// init float -4000.
    /// </summary>
    public float InitFloat4000
    {
        get => StoreVariantExtensions.Get(_initFloat4000);
        set => StoreVariantExtensions.Set(_initFloat4000, value);
    }

    /// <summary>
    /// init float nan.
    /// </summary>
    public float InitFloatNan
    {
        get => StoreVariantExtensions.Get(_initFloatNan);
        set => StoreVariantExtensions.Set(_initFloatNan, value);
    }

    /// <summary>
    /// init float inf.
    /// </summary>
    public float InitFloatInf
    {
        get => StoreVariantExtensions.Get(_initFloatInf);
        set => StoreVariantExtensions.Set(_initFloatInf, value);
    }

    /// <summary>
    /// init float neg inf.
    /// </summary>
    public float InitFloatNegInf
    {
        get => StoreVariantExtensions.Get(_initFloatNegInf);
        set => StoreVariantExtensions.Set(_initFloatNegInf, value);
    }

    /// <summary>
    /// array single.
    /// </summary>
    public float ArraySingle
    {
        get => StoreVariantExtensions.Get(_arraySingle);
        set => StoreVariantExtensions.Set(_arraySingle, value);
    }

    /// <summary>
    /// amp/gain.
    /// </summary>
    public float AmpGain
    {
        get => StoreVariantExtensions.Get(_ampGain);
        set => StoreVariantExtensions.Set(_ampGain, value);
    }

    /// <summary>
    /// amp/offset.
    /// </summary>
    public float AmpOffset
    {
        get => StoreVariantExtensions.Get(_ampOffset);
        set => StoreVariantExtensions.Set(_ampOffset, value);
    }

    /// <summary>
    /// amp/low.
    /// </summary>
    public float AmpLow
    {
        get => StoreVariantExtensions.Get(_ampLow);
        set => StoreVariantExtensions.Set(_ampLow, value);
    }

    /// <summary>
    /// amp/high.
    /// </summary>
    public float AmpHigh
    {
        get => StoreVariantExtensions.Get(_ampHigh);
        set => StoreVariantExtensions.Set(_ampHigh, value);
    }

    /// <summary>
    /// amp/override.
    /// </summary>
    public float AmpOverride
    {
        get => StoreVariantExtensions.Get(_ampOverride);
        set => StoreVariantExtensions.Set(_ampOverride, value);
    }

    /// <summary>
    /// small amp/gain.
    /// </summary>
    public float SmallAmpGain
    {
        get => StoreVariantExtensions.Get(_smallAmpGain);
        set => StoreVariantExtensions.Set(_smallAmpGain, value);
    }

    /// <summary>
    /// small amp/override.
    /// </summary>
    public float SmallAmpOverride
    {
        get => StoreVariantExtensions.Get(_smallAmpOverride);
        set => StoreVariantExtensions.Set(_smallAmpOverride, value);
    }

    /// <summary>
    /// ambiguous amp/gain.
    /// </summary>
    public float AmbiguousAmpGain
    {
        get => StoreVariantExtensions.Get(_ambiguousAmpGain);
        set => StoreVariantExtensions.Set(_ambiguousAmpGain, value);
    }

    /// <summary>
    /// init true.
    /// </summary>
    public bool InitTrue
    {
        get => StoreVariantExtensions.Get(_initTrue);
        set => StoreVariantExtensions.Set(_initTrue, value);
    }

    /// <summary>
    /// init bool 10.
    /// </summary>
    public bool InitBool10
    {
        get => StoreVariantExtensions.Get(_initBool10);
        set => StoreVariantExtensions.Set(_initBool10, value);
    }

    /// <summary>
    /// array bool[0].
    /// </summary>
    public bool ArrayBool0
    {
        get => StoreVariantExtensions.Get(_arrayBool0);
        set => StoreVariantExtensions.Set(_arrayBool0, value);
    }

    /// <summary>
    /// array bool[1].
    /// </summary>
    public bool ArrayBool1
    {
        get => StoreVariantExtensions.Get(_arrayBool1);
        set => StoreVariantExtensions.Set(_arrayBool1, value);
    }

    /// <summary>
    /// amp/enable.
    /// </summary>
    public bool AmpEnable
    {
        get => StoreVariantExtensions.Get(_ampEnable);
        set => StoreVariantExtensions.Set(_ampEnable, value);
    }

    /// <summary>
    /// default string.
    /// </summary>
    public string DefaultString
    {
        get => StoreVariantExtensions.Get(_defaultString);
        set => StoreVariantExtensions.Set(_defaultString, value);
    }

    /// <summary>
    /// default int64.
    /// </summary>
    public long DefaultInt64
    {
        get => StoreVariantExtensions.Get(_defaultInt64);
        set => StoreVariantExtensions.Set(_defaultInt64, value);
    }

    /// <summary>
    /// default uint64.
    /// </summary>
    public ulong DefaultUint64
    {
        get => StoreVariantExtensions.Get(_defaultUint64);
        set => StoreVariantExtensions.Set(_defaultUint64, value);
    }

    /// <summary>
    /// default double.
    /// </summary>
    public double DefaultDouble
    {
        get => StoreVariantExtensions.Get(_defaultDouble);
        set => StoreVariantExtensions.Set(_defaultDouble, value);
    }

    /// <summary>
    /// default ptr64.
    /// </summary>
    public ulong DefaultPtr64
    {
        get => StoreVariantExtensions.Get(_defaultPtr64);
        set => StoreVariantExtensions.Set(_defaultPtr64, value);
    }

    /// <summary>
    /// init string empty.
    /// </summary>
    public string InitStringEmpty
    {
        get => StoreVariantExtensions.Get(_initStringEmpty);
        set => StoreVariantExtensions.Set(_initStringEmpty, value);
    }

    /// <summary>
    /// default blob.
    /// </summary>
    public byte[] DefaultBlob
    {
        get => StoreVariantExtensions.Get(_defaultBlob);
        set => StoreVariantExtensions.Set(_defaultBlob, value);
    }

    /// <summary>
    /// default int32.
    /// </summary>
    public int DefaultInt32
    {
        get => StoreVariantExtensions.Get(_defaultInt32);
        set => StoreVariantExtensions.Set(_defaultInt32, value);
    }

    /// <summary>
    /// default uint32.
    /// </summary>
    public uint DefaultUint32
    {
        get => StoreVariantExtensions.Get(_defaultUint32);
        set => StoreVariantExtensions.Set(_defaultUint32, value);
    }

    /// <summary>
    /// default float.
    /// </summary>
    public float DefaultFloat
    {
        get => StoreVariantExtensions.Get(_defaultFloat);
        set => StoreVariantExtensions.Set(_defaultFloat, value);
    }

    /// <summary>
    /// default ptr32.
    /// </summary>
    public uint DefaultPtr32
    {
        get => StoreVariantExtensions.Get(_defaultPtr32);
        set => StoreVariantExtensions.Set(_defaultPtr32, value);
    }

    /// <summary>
    /// init float 0.
    /// </summary>
    public float InitFloat0
    {
        get => StoreVariantExtensions.Get(_initFloat0);
        set => StoreVariantExtensions.Set(_initFloat0, value);
    }

    /// <summary>
    /// array string[0].
    /// </summary>
    public string ArrayString0
    {
        get => StoreVariantExtensions.Get(_arrayString0);
        set => StoreVariantExtensions.Set(_arrayString0, value);
    }

    /// <summary>
    /// array string[1].
    /// </summary>
    public string ArrayString1
    {
        get => StoreVariantExtensions.Get(_arrayString1);
        set => StoreVariantExtensions.Set(_arrayString1, value);
    }

    /// <summary>
    /// array string[2].
    /// </summary>
    public string ArrayString2
    {
        get => StoreVariantExtensions.Get(_arrayString2);
        set => StoreVariantExtensions.Set(_arrayString2, value);
    }

    /// <summary>
    /// scope/inner int.
    /// </summary>
    public int ScopeInnerInt
    {
        get => StoreVariantExtensions.Get(_scopeInnerInt);
        set => StoreVariantExtensions.Set(_scopeInnerInt, value);
    }

    /// <summary>
    /// value with unit (km/s).
    /// </summary>
    public float ValueWithUnitKmS
    {
        get => StoreVariantExtensions.Get(_valueWithUnitKmS);
        set => StoreVariantExtensions.Set(_valueWithUnitKmS, value);
    }

    /// <summary>
    /// value with complex unit (J/s/m^2).
    /// </summary>
    public float ValueWithComplexUnitJSM2
    {
        get => StoreVariantExtensions.Get(_valueWithComplexUnitJSM2);
        set => StoreVariantExtensions.Set(_valueWithComplexUnitJSM2, value);
    }

    /// <summary>
    /// value with abiguous unit (m/s).
    /// </summary>
    public float ValueWithAbiguousUnitMS
    {
        get => StoreVariantExtensions.Get(_valueWithAbiguousUnitMS);
        set => StoreVariantExtensions.Set(_valueWithAbiguousUnitMS, value);
    }

    /// <summary>
    /// value with abiguous unit (m/h).
    /// </summary>
    public float ValueWithAbiguousUnitMH
    {
        get => StoreVariantExtensions.Get(_valueWithAbiguousUnitMH);
        set => StoreVariantExtensions.Set(_valueWithAbiguousUnitMH, value);
    }

    /// <summary>
    /// amp/input.
    /// </summary>
    public float AmpInput
    {
        get => StoreVariantExtensions.Get(_ampInput);
        set => StoreVariantExtensions.Set(_ampInput, value);
    }

    /// <summary>
    /// amp/output.
    /// </summary>
    public float AmpOutput
    {
        get => StoreVariantExtensions.Get(_ampOutput);
        set => StoreVariantExtensions.Set(_ampOutput, value);
    }

    /// <summary>
    /// small amp/output.
    /// </summary>
    public float SmallAmpOutput
    {
        get => StoreVariantExtensions.Get(_smallAmpOutput);
        set => StoreVariantExtensions.Set(_smallAmpOutput, value);
    }

    /// <summary>
    /// ambiguous amp/output.
    /// </summary>
    public float AmbiguousAmpOutput
    {
        get => StoreVariantExtensions.Get(_ambiguousAmpOutput);
        set => StoreVariantExtensions.Set(_ambiguousAmpOutput, value);
    }

    /// <summary>
    /// default int16.
    /// </summary>
    public short DefaultInt16
    {
        get => StoreVariantExtensions.Get(_defaultInt16);
        set => StoreVariantExtensions.Set(_defaultInt16, value);
    }

    /// <summary>
    /// default uint16.
    /// </summary>
    public ushort DefaultUint16
    {
        get => StoreVariantExtensions.Get(_defaultUint16);
        set => StoreVariantExtensions.Set(_defaultUint16, value);
    }

    /// <summary>
    /// default int8.
    /// </summary>
    public sbyte DefaultInt8
    {
        get => StoreVariantExtensions.Get(_defaultInt8);
        set => StoreVariantExtensions.Set(_defaultInt8, value);
    }

    /// <summary>
    /// default uint8.
    /// </summary>
    public byte DefaultUint8
    {
        get => StoreVariantExtensions.Get(_defaultUint8);
        set => StoreVariantExtensions.Set(_defaultUint8, value);
    }

    /// <summary>
    /// default bool.
    /// </summary>
    public bool DefaultBool
    {
        get => StoreVariantExtensions.Get(_defaultBool);
        set => StoreVariantExtensions.Set(_defaultBool, value);
    }

    /// <summary>
    /// init false.
    /// </summary>
    public bool InitFalse
    {
        get => StoreVariantExtensions.Get(_initFalse);
        set => StoreVariantExtensions.Set(_initFalse, value);
    }

    /// <summary>
    /// init bool 0.
    /// </summary>
    public bool InitBool0
    {
        get => StoreVariantExtensions.Get(_initBool0);
        set => StoreVariantExtensions.Set(_initBool0, value);
    }

    /// <summary>
    /// array bool[2].
    /// </summary>
    public bool ArrayBool2
    {
        get => StoreVariantExtensions.Get(_arrayBool2);
        set => StoreVariantExtensions.Set(_arrayBool2, value);
    }

    /// <summary>
    /// scope/inner bool.
    /// </summary>
    public bool ScopeInnerBool
    {
        get => StoreVariantExtensions.Get(_scopeInnerBool);
        set => StoreVariantExtensions.Set(_scopeInnerBool, value);
    }

    /// <summary>
    /// some other scope/some other inner bool.
    /// </summary>
    public bool SomeOtherScopeSomeOtherInnerBool
    {
        get => StoreVariantExtensions.Get(_someOtherScopeSomeOtherInnerBool);
        set => StoreVariantExtensions.Set(_someOtherScopeSomeOtherInnerBool, value);
    }

    /// <summary>
    /// ambiguous amp/enable.
    /// </summary>
    public bool AmbiguousAmpEnable
    {
        get => StoreVariantExtensions.Get(_ambiguousAmpEnable);
        set => StoreVariantExtensions.Set(_ambiguousAmpEnable, value);
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
            0 => nameof(TestStore.InitString),
            16 => nameof(TestStore.DoubleAmpGain),
            24 => nameof(TestStore.InitDecimal),
            28 => nameof(TestStore.InitNegative),
            32 => nameof(TestStore.InitHex),
            36 => nameof(TestStore.InitBin),
            40 => nameof(TestStore.InitFloat1),
            44 => nameof(TestStore.InitFloat314),
            48 => nameof(TestStore.InitFloat4000),
            52 => nameof(TestStore.InitFloatNan),
            56 => nameof(TestStore.InitFloatInf),
            60 => nameof(TestStore.InitFloatNegInf),
            64 => nameof(TestStore.ArraySingle),
            68 => nameof(TestStore.AmpGain),
            72 => nameof(TestStore.AmpOffset),
            76 => nameof(TestStore.AmpLow),
            80 => nameof(TestStore.AmpHigh),
            84 => nameof(TestStore.AmpOverride),
            88 => nameof(TestStore.SmallAmpGain),
            92 => nameof(TestStore.SmallAmpOverride),
            96 => nameof(TestStore.AmbiguousAmpGain),
            100 => nameof(TestStore.InitTrue),
            101 => nameof(TestStore.InitBool10),
            102 => nameof(TestStore.ArrayBool0),
            103 => nameof(TestStore.ArrayBool1),
            104 => nameof(TestStore.AmpEnable),
            112 => nameof(TestStore.DefaultString),
            128 => nameof(TestStore.DefaultInt64),
            136 => nameof(TestStore.DefaultUint64),
            144 => nameof(TestStore.DefaultDouble),
            152 => nameof(TestStore.DefaultPtr64),
            160 => nameof(TestStore.InitStringEmpty),
            176 => nameof(TestStore.DefaultBlob),
            184 => nameof(TestStore.DefaultInt32),
            188 => nameof(TestStore.DefaultUint32),
            192 => nameof(TestStore.DefaultFloat),
            196 => nameof(TestStore.DefaultPtr32),
            200 => nameof(TestStore.InitFloat0),
            204 => nameof(TestStore.ArrayString0),
            212 => nameof(TestStore.ArrayString1),
            220 => nameof(TestStore.ArrayString2),
            228 => nameof(TestStore.ScopeInnerInt),
            232 => nameof(TestStore.ValueWithUnitKmS),
            236 => nameof(TestStore.ValueWithComplexUnitJSM2),
            240 => nameof(TestStore.ValueWithAbiguousUnitMS),
            244 => nameof(TestStore.ValueWithAbiguousUnitMH),
            248 => nameof(TestStore.AmpInput),
            252 => nameof(TestStore.AmpOutput),
            256 => nameof(TestStore.SmallAmpOutput),
            260 => nameof(TestStore.AmbiguousAmpOutput),
            264 => nameof(TestStore.DefaultInt16),
            266 => nameof(TestStore.DefaultUint16),
            268 => nameof(TestStore.DefaultInt8),
            269 => nameof(TestStore.DefaultUint8),
            270 => nameof(TestStore.DefaultBool),
            271 => nameof(TestStore.InitFalse),
            272 => nameof(TestStore.InitBool0),
            273 => nameof(TestStore.ArrayBool2),
            274 => nameof(TestStore.ScopeInnerBool),
            275 => nameof(TestStore.SomeOtherScopeSomeOtherInnerBool),
            276 => nameof(TestStore.AmbiguousAmpEnable),
            _ => throw new ArgumentOutOfRangeException(nameof(offset), offset, "Unknown offset")
        });

        PropertyChanged?.Invoke(this, args);
    }
}
