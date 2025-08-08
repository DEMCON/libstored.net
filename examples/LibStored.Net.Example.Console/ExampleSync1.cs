// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.ComponentModel;

namespace LibStored.Net.Example.Console;

/// <summary>
/// <code>
/// int32 i
/// double d
/// (bool) sync ExampleSync2
/// </code>
/// </summary>
public class ExampleSync1 :Store, INotifyPropertyChanged
{
    private static readonly byte[] InitialBuffer = [
        0, 0, 0, 0,
        0, 0, 0, 0,
        0x01, 0, 0, 0
    ];

    private readonly byte[] _data = new byte[12];

    private readonly Dictionary<string, DebugVariantInfo> _debugDirectory = [];

    public ExampleSync1()
    {
        ExampleSync1.InitialBuffer.AsSpan().CopyTo(_data.AsSpan());

        D = new StoreVariable<double>(0, 8, this);
        I = new StoreVariable<int>(8, 4, this);

        _debugDirectory.Add("/ExampleSync1/d", new DebugVariantInfo(Types.Double, 0, 8));
        _debugDirectory.Add("/ExampleSync1/i", new DebugVariantInfo(Types.Int32, 8, 4));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override Span<byte> GetBuffer() => _data;
    public override IReadOnlyDictionary<string, DebugVariantInfo> GetDebugVariants() => _debugDirectory;

    public override string Name => "/ExampleSync1";
    public override string Hash => "681a3ece584568efcf5879a64b688fc19e620577";
    public override int VariableCount => 7;

    public StoreVariable<int> I { get; }
    public StoreVariable<double> D { get; }

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

        PropertyChangedEventArgs args = new PropertyChangedEventArgs(offset switch
        {
            0 => nameof(ExampleSync1.D),
            8 => nameof(ExampleSync1.I),
            _ => throw new ArgumentOutOfRangeException(nameof(offset), offset, "Unknown offset")
        });

        PropertyChanged?.Invoke(this, args);
    }
}
