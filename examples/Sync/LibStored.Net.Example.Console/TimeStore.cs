// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

namespace LibStored.Net.Example.Console;

public class TimeStore : Store
{
    private readonly byte[] _data = new byte[4];
    private readonly StoreVariable<uint> _t;
    private readonly Dictionary<string, DebugVariantInfo> _debugDirectory = [];

    public TimeStore()
    {
        _t = new StoreVariable<uint>(0, 4, this);
        _debugDirectory.Add("/t (ms)", new DebugVariantInfo(Types.Uint32, 0, 4));
    }

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, DebugVariantInfo> GetDebugVariants() => _debugDirectory;

    /// <inheritdoc />
    public override string Name => "/TimeStore";

    /// <inheritdoc />
    public override string Hash => "";

    /// <inheritdoc />
    public override int VariableCount => 1;

    /// <inheritdoc />
    public override Span<byte> GetBuffer() => _data;

    /// <summary>
    /// /t (ms).
    /// Call this to update the time. The value is in milliseconds.
    /// Update this just before a calls to <see cref="Debugger.Trace"/>>.
    /// </summary>
    public uint T
    {
        get => _t.Get();
        set => _t.Set(value);
    }


    /// <inheritdoc />
    public override void Changed(int offset)
    {
    }
}
