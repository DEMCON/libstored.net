#nullable enable

namespace LibStored.Net.Tests;

/// <summary>
/// TestStore generated from TestStoreMeta.py.
/// </summary>
public class TestStore : global::LibStored.Net.Store, global::System.ComponentModel.INotifyPropertyChanged
{
    private static readonly byte[] InitialBuffer = [
        0x61, 0x20, 0x62, 0x22, 0x63, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0xc0, 0x2a, 0x00, 0x00, 0x00, 0xd6, 0xff, 0xff, 0xff, 0x54, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f, 0xc3, 0xf5, 0x48, 0x40, 0x00, 0x00, 0x7a, 0xc5, 0x00, 0x00, 0xc0, 0x7f, 0x00, 0x00, 0x80, 0x7f, 0x00, 0x00, 0x80, 0xff, 0x00, 0x00, 0x40, 0x40, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x3f, 0x00, 0x00, 0x80, 0xbf, 0x00, 0x00, 0x20, 0x41, 0x00, 0x00, 0xc0, 0x7f, 0x00, 0x00, 0x60, 0x40, 0x00, 0x00, 0xc0, 0x7f, 0x00, 0x00, 0x80, 0xbf, 0x01, 0x01, 0x01, 0x01, 0x01
    ];

    private readonly byte[] _data = new byte[277];

    private readonly global::System.Collections.Generic.Dictionary<string, global::LibStored.Net.DebugVariantInfo> _debugDirectory = [];

    private readonly global::LibStored.Net.StoreVariable<sbyte> _defaultInt8;
    private readonly global::LibStored.Net.StoreVariable<short> _defaultInt16;
    private readonly global::LibStored.Net.StoreVariable<int> _defaultInt32;
    private readonly global::LibStored.Net.StoreVariable<long> _defaultInt64;
    private readonly global::LibStored.Net.StoreVariable<byte> _defaultUint8;
    private readonly global::LibStored.Net.StoreVariable<ushort> _defaultUint16;
    private readonly global::LibStored.Net.StoreVariable<uint> _defaultUint32;
    private readonly global::LibStored.Net.StoreVariable<ulong> _defaultUint64;
    private readonly global::LibStored.Net.StoreVariable<float> _defaultFloat;
    private readonly global::LibStored.Net.StoreVariable<double> _defaultDouble;
    private readonly global::LibStored.Net.StoreVariable<bool> _defaultBool;
    private readonly global::LibStored.Net.StoreVariable<uint> _defaultPtr32;
    private readonly global::LibStored.Net.StoreVariable<ulong> _defaultPtr64;
    private readonly global::LibStored.Net.StoreVariant<byte[]> _defaultBlob;
    private readonly global::LibStored.Net.StoreVariant<string> _defaultString;
    private readonly global::LibStored.Net.StoreVariable<int> _initDecimal;
    private readonly global::LibStored.Net.StoreVariable<int> _initNegative;
    private readonly global::LibStored.Net.StoreVariable<int> _initHex;
    private readonly global::LibStored.Net.StoreVariable<int> _initBin;
    private readonly global::LibStored.Net.StoreVariable<bool> _initTrue;
    private readonly global::LibStored.Net.StoreVariable<bool> _initFalse;
    private readonly global::LibStored.Net.StoreVariable<bool> _initBool0;
    private readonly global::LibStored.Net.StoreVariable<bool> _initBool10;
    private readonly global::LibStored.Net.StoreVariable<float> _initFloat0;
    private readonly global::LibStored.Net.StoreVariable<float> _initFloat1;
    private readonly global::LibStored.Net.StoreVariable<float> _initFloat314;
    private readonly global::LibStored.Net.StoreVariable<float> _initFloat4000;
    private readonly global::LibStored.Net.StoreVariable<float> _initFloatNan;
    private readonly global::LibStored.Net.StoreVariable<float> _initFloatInf;
    private readonly global::LibStored.Net.StoreVariable<float> _initFloatNegInf;
    private readonly global::LibStored.Net.StoreVariant<string> _initString;
    private readonly global::LibStored.Net.StoreVariant<string> _initStringEmpty;
    private readonly global::LibStored.Net.StoreVariable<bool> _arrayBool0;
    private readonly global::LibStored.Net.StoreVariable<bool> _arrayBool1;
    private readonly global::LibStored.Net.StoreVariable<bool> _arrayBool2;
    private readonly global::LibStored.Net.StoreVariant<string> _arrayString0;
    private readonly global::LibStored.Net.StoreVariant<string> _arrayString1;
    private readonly global::LibStored.Net.StoreVariant<string> _arrayString2;
    private readonly global::LibStored.Net.StoreVariable<float> _arraySingle;
    private readonly global::LibStored.Net.StoreVariable<bool> _scopeInnerBool;
    private readonly global::LibStored.Net.StoreVariable<int> _scopeInnerInt;
    private readonly global::LibStored.Net.StoreVariable<bool> _someOtherScopeSomeOtherInnerBool;
    private readonly global::LibStored.Net.StoreVariable<float> _valueWithUnitKmS;
    private readonly global::LibStored.Net.StoreVariable<float> _valueWithComplexUnitJSM2;
    private readonly global::LibStored.Net.StoreVariable<float> _valueWithAbiguousUnitMS;
    private readonly global::LibStored.Net.StoreVariable<float> _valueWithAbiguousUnitMH;
    private readonly global::LibStored.Net.StoreVariable<float> _ampInput;
    private readonly global::LibStored.Net.StoreVariable<bool> _ampEnable;
    private readonly global::LibStored.Net.StoreVariable<float> _ampGain;
    private readonly global::LibStored.Net.StoreVariable<float> _ampOffset;
    private readonly global::LibStored.Net.StoreVariable<float> _ampLow;
    private readonly global::LibStored.Net.StoreVariable<float> _ampHigh;
    private readonly global::LibStored.Net.StoreVariable<float> _ampOverride;
    private readonly global::LibStored.Net.StoreVariable<float> _ampOutput;
    private readonly global::LibStored.Net.StoreVariable<float> _smallAmpGain;
    private readonly global::LibStored.Net.StoreVariable<float> _smallAmpOverride;
    private readonly global::LibStored.Net.StoreVariable<float> _smallAmpOutput;
    private readonly global::LibStored.Net.StoreVariable<float> _ambiguousAmpGain;
    private readonly global::LibStored.Net.StoreVariable<bool> _ambiguousAmpEnable;
    private readonly global::LibStored.Net.StoreVariable<float> _ambiguousAmpOutput;
    private readonly global::LibStored.Net.StoreVariable<double> _doubleAmpGain;

    public TestStore()
    {
        TestStore.InitialBuffer.AsSpan().CopyTo(_data.AsSpan());

        _defaultInt8 = new global::LibStored.Net.StoreVariable<sbyte>(268, 1, this);
        _defaultInt16 = new global::LibStored.Net.StoreVariable<short>(264, 2, this);
        _defaultInt32 = new global::LibStored.Net.StoreVariable<int>(184, 4, this);
        _defaultInt64 = new global::LibStored.Net.StoreVariable<long>(128, 8, this);
        _defaultUint8 = new global::LibStored.Net.StoreVariable<byte>(269, 1, this);
        _defaultUint16 = new global::LibStored.Net.StoreVariable<ushort>(266, 2, this);
        _defaultUint32 = new global::LibStored.Net.StoreVariable<uint>(188, 4, this);
        _defaultUint64 = new global::LibStored.Net.StoreVariable<ulong>(136, 8, this);
        _defaultFloat = new global::LibStored.Net.StoreVariable<float>(192, 4, this);
        _defaultDouble = new global::LibStored.Net.StoreVariable<double>(144, 8, this);
        _defaultBool = new global::LibStored.Net.StoreVariable<bool>(270, 1, this);
        _defaultPtr32 = new global::LibStored.Net.StoreVariable<uint>(196, 4, this);
        _defaultPtr64 = new global::LibStored.Net.StoreVariable<ulong>(152, 8, this);
        _defaultBlob = new global::LibStored.Net.StoreVariant<byte[]>(176, 5, this);
        _defaultString = new global::LibStored.Net.StoreVariant<string>(112, 10, this);
        _initDecimal = new global::LibStored.Net.StoreVariable<int>(24, 4, this);
        _initNegative = new global::LibStored.Net.StoreVariable<int>(28, 4, this);
        _initHex = new global::LibStored.Net.StoreVariable<int>(32, 4, this);
        _initBin = new global::LibStored.Net.StoreVariable<int>(36, 4, this);
        _initTrue = new global::LibStored.Net.StoreVariable<bool>(100, 1, this);
        _initFalse = new global::LibStored.Net.StoreVariable<bool>(271, 1, this);
        _initBool0 = new global::LibStored.Net.StoreVariable<bool>(272, 1, this);
        _initBool10 = new global::LibStored.Net.StoreVariable<bool>(101, 1, this);
        _initFloat0 = new global::LibStored.Net.StoreVariable<float>(200, 4, this);
        _initFloat1 = new global::LibStored.Net.StoreVariable<float>(40, 4, this);
        _initFloat314 = new global::LibStored.Net.StoreVariable<float>(44, 4, this);
        _initFloat4000 = new global::LibStored.Net.StoreVariable<float>(48, 4, this);
        _initFloatNan = new global::LibStored.Net.StoreVariable<float>(52, 4, this);
        _initFloatInf = new global::LibStored.Net.StoreVariable<float>(56, 4, this);
        _initFloatNegInf = new global::LibStored.Net.StoreVariable<float>(60, 4, this);
        _initString = new global::LibStored.Net.StoreVariant<string>(0, 8, this);
        _initStringEmpty = new global::LibStored.Net.StoreVariant<string>(160, 8, this);
        _arrayBool0 = new global::LibStored.Net.StoreVariable<bool>(102, 1, this);
        _arrayBool1 = new global::LibStored.Net.StoreVariable<bool>(103, 1, this);
        _arrayBool2 = new global::LibStored.Net.StoreVariable<bool>(273, 1, this);
        _arrayString0 = new global::LibStored.Net.StoreVariant<string>(204, 4, this);
        _arrayString1 = new global::LibStored.Net.StoreVariant<string>(212, 4, this);
        _arrayString2 = new global::LibStored.Net.StoreVariant<string>(220, 4, this);
        _arraySingle = new global::LibStored.Net.StoreVariable<float>(64, 4, this);
        _scopeInnerBool = new global::LibStored.Net.StoreVariable<bool>(274, 1, this);
        _scopeInnerInt = new global::LibStored.Net.StoreVariable<int>(228, 4, this);
        _someOtherScopeSomeOtherInnerBool = new global::LibStored.Net.StoreVariable<bool>(275, 1, this);
        _valueWithUnitKmS = new global::LibStored.Net.StoreVariable<float>(232, 4, this);
        _valueWithComplexUnitJSM2 = new global::LibStored.Net.StoreVariable<float>(236, 4, this);
        _valueWithAbiguousUnitMS = new global::LibStored.Net.StoreVariable<float>(240, 4, this);
        _valueWithAbiguousUnitMH = new global::LibStored.Net.StoreVariable<float>(244, 4, this);
        _ampInput = new global::LibStored.Net.StoreVariable<float>(248, 4, this);
        _ampEnable = new global::LibStored.Net.StoreVariable<bool>(104, 1, this);
        _ampGain = new global::LibStored.Net.StoreVariable<float>(68, 4, this);
        _ampOffset = new global::LibStored.Net.StoreVariable<float>(72, 4, this);
        _ampLow = new global::LibStored.Net.StoreVariable<float>(76, 4, this);
        _ampHigh = new global::LibStored.Net.StoreVariable<float>(80, 4, this);
        _ampOverride = new global::LibStored.Net.StoreVariable<float>(84, 4, this);
        _ampOutput = new global::LibStored.Net.StoreVariable<float>(252, 4, this);
        _smallAmpGain = new global::LibStored.Net.StoreVariable<float>(88, 4, this);
        _smallAmpOverride = new global::LibStored.Net.StoreVariable<float>(92, 4, this);
        _smallAmpOutput = new global::LibStored.Net.StoreVariable<float>(256, 4, this);
        _ambiguousAmpGain = new global::LibStored.Net.StoreVariable<float>(96, 4, this);
        _ambiguousAmpEnable = new global::LibStored.Net.StoreVariable<bool>(276, 1, this);
        _ambiguousAmpOutput = new global::LibStored.Net.StoreVariable<float>(260, 4, this);
        _doubleAmpGain = new global::LibStored.Net.StoreVariable<double>(16, 8, this);

        _debugDirectory.Add("/TestStore/default int8", new global::LibStored.Net.DebugVariantInfo(Types.Int8, 268, 1));
        _debugDirectory.Add("/TestStore/default int16", new global::LibStored.Net.DebugVariantInfo(Types.Int16, 264, 2));
        _debugDirectory.Add("/TestStore/default int32", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 184, 4));
        _debugDirectory.Add("/TestStore/default int64", new global::LibStored.Net.DebugVariantInfo(Types.Int64, 128, 8));
        _debugDirectory.Add("/TestStore/default uint8", new global::LibStored.Net.DebugVariantInfo(Types.Uint8, 269, 1));
        _debugDirectory.Add("/TestStore/default uint16", new global::LibStored.Net.DebugVariantInfo(Types.Uint16, 266, 2));
        _debugDirectory.Add("/TestStore/default uint32", new global::LibStored.Net.DebugVariantInfo(Types.Uint32, 188, 4));
        _debugDirectory.Add("/TestStore/default uint64", new global::LibStored.Net.DebugVariantInfo(Types.Uint64, 136, 8));
        _debugDirectory.Add("/TestStore/default float", new global::LibStored.Net.DebugVariantInfo(Types.Float, 192, 4));
        _debugDirectory.Add("/TestStore/default double", new global::LibStored.Net.DebugVariantInfo(Types.Double, 144, 8));
        _debugDirectory.Add("/TestStore/default bool", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 270, 1));
        _debugDirectory.Add("/TestStore/default ptr32", new global::LibStored.Net.DebugVariantInfo(Types.Pointer32, 196, 4));
        _debugDirectory.Add("/TestStore/default ptr64", new global::LibStored.Net.DebugVariantInfo(Types.Pointer64, 152, 8));
        _debugDirectory.Add("/TestStore/default blob", new global::LibStored.Net.DebugVariantInfo(Types.Blob, 176, 5));
        _debugDirectory.Add("/TestStore/default string", new global::LibStored.Net.DebugVariantInfo(Types.String, 112, 10));
        _debugDirectory.Add("/TestStore/init decimal", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 24, 4));
        _debugDirectory.Add("/TestStore/init negative", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 28, 4));
        _debugDirectory.Add("/TestStore/init hex", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 32, 4));
        _debugDirectory.Add("/TestStore/init bin", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 36, 4));
        _debugDirectory.Add("/TestStore/init true", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 100, 1));
        _debugDirectory.Add("/TestStore/init false", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 271, 1));
        _debugDirectory.Add("/TestStore/init bool 0", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 272, 1));
        _debugDirectory.Add("/TestStore/init bool 10", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 101, 1));
        _debugDirectory.Add("/TestStore/init float 0", new global::LibStored.Net.DebugVariantInfo(Types.Float, 200, 4));
        _debugDirectory.Add("/TestStore/init float 1", new global::LibStored.Net.DebugVariantInfo(Types.Float, 40, 4));
        _debugDirectory.Add("/TestStore/init float 3.14", new global::LibStored.Net.DebugVariantInfo(Types.Float, 44, 4));
        _debugDirectory.Add("/TestStore/init float -4000", new global::LibStored.Net.DebugVariantInfo(Types.Float, 48, 4));
        _debugDirectory.Add("/TestStore/init float nan", new global::LibStored.Net.DebugVariantInfo(Types.Float, 52, 4));
        _debugDirectory.Add("/TestStore/init float inf", new global::LibStored.Net.DebugVariantInfo(Types.Float, 56, 4));
        _debugDirectory.Add("/TestStore/init float neg inf", new global::LibStored.Net.DebugVariantInfo(Types.Float, 60, 4));
        _debugDirectory.Add("/TestStore/init string", new global::LibStored.Net.DebugVariantInfo(Types.String, 0, 8));
        _debugDirectory.Add("/TestStore/init string empty", new global::LibStored.Net.DebugVariantInfo(Types.String, 160, 8));
        _debugDirectory.Add("/TestStore/array bool[0]", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 102, 1));
        _debugDirectory.Add("/TestStore/array bool[1]", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 103, 1));
        _debugDirectory.Add("/TestStore/array bool[2]", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 273, 1));
        _debugDirectory.Add("/TestStore/array string[0]", new global::LibStored.Net.DebugVariantInfo(Types.String, 204, 4));
        _debugDirectory.Add("/TestStore/array string[1]", new global::LibStored.Net.DebugVariantInfo(Types.String, 212, 4));
        _debugDirectory.Add("/TestStore/array string[2]", new global::LibStored.Net.DebugVariantInfo(Types.String, 220, 4));
        _debugDirectory.Add("/TestStore/array single", new global::LibStored.Net.DebugVariantInfo(Types.Float, 64, 4));
        _debugDirectory.Add("/TestStore/scope/inner bool", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 274, 1));
        _debugDirectory.Add("/TestStore/scope/inner int", new global::LibStored.Net.DebugVariantInfo(Types.Int32, 228, 4));
        _debugDirectory.Add("/TestStore/some other scope/some other inner bool", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 275, 1));
        _debugDirectory.Add("/TestStore/value with unit (km/s)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 232, 4));
        _debugDirectory.Add("/TestStore/value with complex unit (J/s/m^2)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 236, 4));
        _debugDirectory.Add("/TestStore/value with abiguous unit (m/s)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 240, 4));
        _debugDirectory.Add("/TestStore/value with abiguous unit (m/h)", new global::LibStored.Net.DebugVariantInfo(Types.Float, 244, 4));
        _debugDirectory.Add("/TestStore/amp/input", new global::LibStored.Net.DebugVariantInfo(Types.Float, 248, 4));
        _debugDirectory.Add("/TestStore/amp/enable", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 104, 1));
        _debugDirectory.Add("/TestStore/amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Float, 68, 4));
        _debugDirectory.Add("/TestStore/amp/offset", new global::LibStored.Net.DebugVariantInfo(Types.Float, 72, 4));
        _debugDirectory.Add("/TestStore/amp/low", new global::LibStored.Net.DebugVariantInfo(Types.Float, 76, 4));
        _debugDirectory.Add("/TestStore/amp/high", new global::LibStored.Net.DebugVariantInfo(Types.Float, 80, 4));
        _debugDirectory.Add("/TestStore/amp/override", new global::LibStored.Net.DebugVariantInfo(Types.Float, 84, 4));
        _debugDirectory.Add("/TestStore/amp/output", new global::LibStored.Net.DebugVariantInfo(Types.Float, 252, 4));
        _debugDirectory.Add("/TestStore/small amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Float, 88, 4));
        _debugDirectory.Add("/TestStore/small amp/override", new global::LibStored.Net.DebugVariantInfo(Types.Float, 92, 4));
        _debugDirectory.Add("/TestStore/small amp/output", new global::LibStored.Net.DebugVariantInfo(Types.Float, 256, 4));
        _debugDirectory.Add("/TestStore/ambiguous amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Float, 96, 4));
        _debugDirectory.Add("/TestStore/ambiguous amp/enable", new global::LibStored.Net.DebugVariantInfo(Types.Bool, 276, 1));
        _debugDirectory.Add("/TestStore/ambiguous amp/output", new global::LibStored.Net.DebugVariantInfo(Types.Float, 260, 4));
        _debugDirectory.Add("/TestStore/double amp/gain", new global::LibStored.Net.DebugVariantInfo(Types.Double, 16, 8));
    }

    public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public override global::System.Span<byte> GetBuffer() => _data;
    public override global::System.Collections.Generic.IReadOnlyDictionary<string, global::LibStored.Net.DebugVariantInfo> GetDebugVariants() => _debugDirectory;

    public override string Name => "/TestStore";
    public override string Hash => "b0bba06dabc9dff2a4c18c4ebdcc39e1418575b4";
    public override int VariableCount => 61;

    /// <summary>
    /// default int8.
    /// </summary>
    public sbyte DefaultInt8 
    { 
        get => _defaultInt8.Get(); 
        set => _defaultInt8.Set(value); 
    }

    /// <summary>
    /// default int16.
    /// </summary>
    public short DefaultInt16 
    { 
        get => _defaultInt16.Get(); 
        set => _defaultInt16.Set(value); 
    }

    /// <summary>
    /// default int32.
    /// </summary>
    public int DefaultInt32 
    { 
        get => _defaultInt32.Get(); 
        set => _defaultInt32.Set(value); 
    }

    /// <summary>
    /// default int64.
    /// </summary>
    public long DefaultInt64 
    { 
        get => _defaultInt64.Get(); 
        set => _defaultInt64.Set(value); 
    }

    /// <summary>
    /// default uint8.
    /// </summary>
    public byte DefaultUint8 
    { 
        get => _defaultUint8.Get(); 
        set => _defaultUint8.Set(value); 
    }

    /// <summary>
    /// default uint16.
    /// </summary>
    public ushort DefaultUint16 
    { 
        get => _defaultUint16.Get(); 
        set => _defaultUint16.Set(value); 
    }

    /// <summary>
    /// default uint32.
    /// </summary>
    public uint DefaultUint32 
    { 
        get => _defaultUint32.Get(); 
        set => _defaultUint32.Set(value); 
    }

    /// <summary>
    /// default uint64.
    /// </summary>
    public ulong DefaultUint64 
    { 
        get => _defaultUint64.Get(); 
        set => _defaultUint64.Set(value); 
    }

    /// <summary>
    /// default float.
    /// </summary>
    public float DefaultFloat 
    { 
        get => _defaultFloat.Get(); 
        set => _defaultFloat.Set(value); 
    }

    /// <summary>
    /// default double.
    /// </summary>
    public double DefaultDouble 
    { 
        get => _defaultDouble.Get(); 
        set => _defaultDouble.Set(value); 
    }

    /// <summary>
    /// default bool.
    /// </summary>
    public bool DefaultBool 
    { 
        get => _defaultBool.Get(); 
        set => _defaultBool.Set(value); 
    }

    /// <summary>
    /// default ptr32.
    /// </summary>
    public uint DefaultPtr32 
    { 
        get => _defaultPtr32.Get(); 
        set => _defaultPtr32.Set(value); 
    }

    /// <summary>
    /// default ptr64.
    /// </summary>
    public ulong DefaultPtr64 
    { 
        get => _defaultPtr64.Get(); 
        set => _defaultPtr64.Set(value); 
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
    /// default string.
    /// </summary>
    public string DefaultString 
    { 
        get => StoreVariantExtensions.Get(_defaultString); 
        set => StoreVariantExtensions.Set(_defaultString, value); 
    }

    /// <summary>
    /// init decimal.
    /// </summary>
    public int InitDecimal 
    { 
        get => _initDecimal.Get(); 
        set => _initDecimal.Set(value); 
    }

    /// <summary>
    /// init negative.
    /// </summary>
    public int InitNegative 
    { 
        get => _initNegative.Get(); 
        set => _initNegative.Set(value); 
    }

    /// <summary>
    /// init hex.
    /// </summary>
    public int InitHex 
    { 
        get => _initHex.Get(); 
        set => _initHex.Set(value); 
    }

    /// <summary>
    /// init bin.
    /// </summary>
    public int InitBin 
    { 
        get => _initBin.Get(); 
        set => _initBin.Set(value); 
    }

    /// <summary>
    /// init true.
    /// </summary>
    public bool InitTrue 
    { 
        get => _initTrue.Get(); 
        set => _initTrue.Set(value); 
    }

    /// <summary>
    /// init false.
    /// </summary>
    public bool InitFalse 
    { 
        get => _initFalse.Get(); 
        set => _initFalse.Set(value); 
    }

    /// <summary>
    /// init bool 0.
    /// </summary>
    public bool InitBool0 
    { 
        get => _initBool0.Get(); 
        set => _initBool0.Set(value); 
    }

    /// <summary>
    /// init bool 10.
    /// </summary>
    public bool InitBool10 
    { 
        get => _initBool10.Get(); 
        set => _initBool10.Set(value); 
    }

    /// <summary>
    /// init float 0.
    /// </summary>
    public float InitFloat0 
    { 
        get => _initFloat0.Get(); 
        set => _initFloat0.Set(value); 
    }

    /// <summary>
    /// init float 1.
    /// </summary>
    public float InitFloat1 
    { 
        get => _initFloat1.Get(); 
        set => _initFloat1.Set(value); 
    }

    /// <summary>
    /// init float 3.14.
    /// </summary>
    public float InitFloat314 
    { 
        get => _initFloat314.Get(); 
        set => _initFloat314.Set(value); 
    }

    /// <summary>
    /// init float -4000.
    /// </summary>
    public float InitFloat4000 
    { 
        get => _initFloat4000.Get(); 
        set => _initFloat4000.Set(value); 
    }

    /// <summary>
    /// init float nan.
    /// </summary>
    public float InitFloatNan 
    { 
        get => _initFloatNan.Get(); 
        set => _initFloatNan.Set(value); 
    }

    /// <summary>
    /// init float inf.
    /// </summary>
    public float InitFloatInf 
    { 
        get => _initFloatInf.Get(); 
        set => _initFloatInf.Set(value); 
    }

    /// <summary>
    /// init float neg inf.
    /// </summary>
    public float InitFloatNegInf 
    { 
        get => _initFloatNegInf.Get(); 
        set => _initFloatNegInf.Set(value); 
    }

    /// <summary>
    /// init string.
    /// </summary>
    public string InitString 
    { 
        get => StoreVariantExtensions.Get(_initString); 
        set => StoreVariantExtensions.Set(_initString, value); 
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
    /// array bool[0].
    /// </summary>
    public bool ArrayBool0 
    { 
        get => _arrayBool0.Get(); 
        set => _arrayBool0.Set(value); 
    }

    /// <summary>
    /// array bool[1].
    /// </summary>
    public bool ArrayBool1 
    { 
        get => _arrayBool1.Get(); 
        set => _arrayBool1.Set(value); 
    }

    /// <summary>
    /// array bool[2].
    /// </summary>
    public bool ArrayBool2 
    { 
        get => _arrayBool2.Get(); 
        set => _arrayBool2.Set(value); 
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
    /// array single.
    /// </summary>
    public float ArraySingle 
    { 
        get => _arraySingle.Get(); 
        set => _arraySingle.Set(value); 
    }

    /// <summary>
    /// scope/inner bool.
    /// </summary>
    public bool ScopeInnerBool 
    { 
        get => _scopeInnerBool.Get(); 
        set => _scopeInnerBool.Set(value); 
    }

    /// <summary>
    /// scope/inner int.
    /// </summary>
    public int ScopeInnerInt 
    { 
        get => _scopeInnerInt.Get(); 
        set => _scopeInnerInt.Set(value); 
    }

    /// <summary>
    /// some other scope/some other inner bool.
    /// </summary>
    public bool SomeOtherScopeSomeOtherInnerBool 
    { 
        get => _someOtherScopeSomeOtherInnerBool.Get(); 
        set => _someOtherScopeSomeOtherInnerBool.Set(value); 
    }

    /// <summary>
    /// value with unit (km/s).
    /// </summary>
    public float ValueWithUnitKmS 
    { 
        get => _valueWithUnitKmS.Get(); 
        set => _valueWithUnitKmS.Set(value); 
    }

    /// <summary>
    /// value with complex unit (J/s/m^2).
    /// </summary>
    public float ValueWithComplexUnitJSM2 
    { 
        get => _valueWithComplexUnitJSM2.Get(); 
        set => _valueWithComplexUnitJSM2.Set(value); 
    }

    /// <summary>
    /// value with abiguous unit (m/s).
    /// </summary>
    public float ValueWithAbiguousUnitMS 
    { 
        get => _valueWithAbiguousUnitMS.Get(); 
        set => _valueWithAbiguousUnitMS.Set(value); 
    }

    /// <summary>
    /// value with abiguous unit (m/h).
    /// </summary>
    public float ValueWithAbiguousUnitMH 
    { 
        get => _valueWithAbiguousUnitMH.Get(); 
        set => _valueWithAbiguousUnitMH.Set(value); 
    }

    /// <summary>
    /// amp/input.
    /// </summary>
    public float AmpInput 
    { 
        get => _ampInput.Get(); 
        set => _ampInput.Set(value); 
    }

    /// <summary>
    /// amp/enable.
    /// </summary>
    public bool AmpEnable 
    { 
        get => _ampEnable.Get(); 
        set => _ampEnable.Set(value); 
    }

    /// <summary>
    /// amp/gain.
    /// </summary>
    public float AmpGain 
    { 
        get => _ampGain.Get(); 
        set => _ampGain.Set(value); 
    }

    /// <summary>
    /// amp/offset.
    /// </summary>
    public float AmpOffset 
    { 
        get => _ampOffset.Get(); 
        set => _ampOffset.Set(value); 
    }

    /// <summary>
    /// amp/low.
    /// </summary>
    public float AmpLow 
    { 
        get => _ampLow.Get(); 
        set => _ampLow.Set(value); 
    }

    /// <summary>
    /// amp/high.
    /// </summary>
    public float AmpHigh 
    { 
        get => _ampHigh.Get(); 
        set => _ampHigh.Set(value); 
    }

    /// <summary>
    /// amp/override.
    /// </summary>
    public float AmpOverride 
    { 
        get => _ampOverride.Get(); 
        set => _ampOverride.Set(value); 
    }

    /// <summary>
    /// amp/output.
    /// </summary>
    public float AmpOutput 
    { 
        get => _ampOutput.Get(); 
        set => _ampOutput.Set(value); 
    }

    /// <summary>
    /// small amp/gain.
    /// </summary>
    public float SmallAmpGain 
    { 
        get => _smallAmpGain.Get(); 
        set => _smallAmpGain.Set(value); 
    }

    /// <summary>
    /// small amp/override.
    /// </summary>
    public float SmallAmpOverride 
    { 
        get => _smallAmpOverride.Get(); 
        set => _smallAmpOverride.Set(value); 
    }

    /// <summary>
    /// small amp/output.
    /// </summary>
    public float SmallAmpOutput 
    { 
        get => _smallAmpOutput.Get(); 
        set => _smallAmpOutput.Set(value); 
    }

    /// <summary>
    /// ambiguous amp/gain.
    /// </summary>
    public float AmbiguousAmpGain 
    { 
        get => _ambiguousAmpGain.Get(); 
        set => _ambiguousAmpGain.Set(value); 
    }

    /// <summary>
    /// ambiguous amp/enable.
    /// </summary>
    public bool AmbiguousAmpEnable 
    { 
        get => _ambiguousAmpEnable.Get(); 
        set => _ambiguousAmpEnable.Set(value); 
    }

    /// <summary>
    /// ambiguous amp/output.
    /// </summary>
    public float AmbiguousAmpOutput 
    { 
        get => _ambiguousAmpOutput.Get(); 
        set => _ambiguousAmpOutput.Set(value); 
    }

    /// <summary>
    /// double amp/gain.
    /// </summary>
    public double DoubleAmpGain 
    { 
        get => _doubleAmpGain.Get(); 
        set => _doubleAmpGain.Set(value); 
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