// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Stream = LibStored.Net.Debugging.Stream;

namespace LibStored.Net.Debugging;

/// <summary>
/// Provides a protocol layer for debugging and interacting with mapped <see cref="Store"/> objects.
/// </summary>
public class Debugger : Protocol.ProtocolLayer
{
    private const char CmdCapabilities = '?';
    private const char CmdRead = 'r';
    private const char CmdWrite = 'w';
    private const char CmdEcho = 'e';
    private const char CmdList = 'l';
    private const char CmdAlias = 'a';
    private const char CmdMacro = 'm';
    private const char CmdIdentification = 'i';
    private const char CmdVersion = 'v';
    private const char CmdReadMem = 'R';
    private const char CmdWriteMem = 'W';
    private const char CmdStream = 's';
    private const char CmdTrace = 't';
    private const char CmdFlush = 'f';
    private const char Ack = '!';
    private const char Nack = '?';
    private const int _version = 2;

    private readonly Dictionary<string, Store> _stores = [];
    private readonly Dictionary<char, DebugVariant> _aliases = [];
    private readonly Dictionary<char, byte[]> _macros = [];
    private readonly Dictionary<char, Stream> _streams = [];
    private readonly int _maxMacrosSize;
    private readonly int _maxStreamCount;

    private string? _identification;
    private string _versions;
    private ulong _traceDecimation;
    private ulong _traceCount;
    private char _traceMacro;
    private char _traceStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="Debugger"/> class.
    /// </summary>
    /// <param name="identification">Optional identification string for the debugger.</param>
    /// <param name="versions">Optional version string for the application.</param>
    /// <param name="maxMacrosSize">Combined size of all macros</param>
    /// <param name="maxStreamCount"></param>
    public Debugger(string? identification = null, string versions = "", int maxMacrosSize = 4096, int maxStreamCount = 2)
    {
        _identification = identification;
        _versions = versions;
        _maxMacrosSize = maxMacrosSize;
        _maxStreamCount = maxStreamCount;
    }

    /// <summary>
    /// Gets or sets the identification string for this debugger instance.
    /// </summary>
    public string? Identification
    {
        get => _identification;
        set => _identification = value;
    }

    /// <summary>
    /// Gets or sets the application-defined version string.
    /// </summary>
    public string Versions
    {
        get => _versions;
        set => _versions = value;
    }


    /// <summary>
    /// Maps a <see cref="Store"/> to a specified name for debugging access.
    /// </summary>
    /// <param name="store">The store to map.</param>
    /// <param name="name">Optional name for the store. If null, uses <see cref="Store.Name"/>.</param>
    public void Map(Store store, string? name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = store.Name;
        }

        if (string.IsNullOrEmpty(name) || name[0] != '/' || name.AsSpan().Slice(1).Contains('/'))
        {
            return;
        }
        _stores[name] = store;
    }

    /// <summary>
    /// Unmaps a previously mapped <see cref="Store"/> by name.
    /// </summary>
    /// <param name="name">The name of the store to unmap.</param>
    public void Unmap(string name) => _stores.Remove(name);

    /// <summary>
    /// Lists all debug variants in all mapped stores, invoking the specified action for each.
    /// </summary>
    /// <param name="action">The action to invoke for each variant, with the name and <see cref="DebugVariant"/>.</param>
    public void List(Action<string, DebugVariant> action)
    {
        foreach (KeyValuePair<string, Store> store in _stores)
        {
            store.Value.List(action, store.Key);
        }
    }

    /// <summary>
    /// Finds a debug variant by its name.
    /// </summary>
    /// <param name="name">The name of the variant to find.</param>
    /// <returns>The <see cref="DebugVariant"/> if found; otherwise, null.</returns>
    public DebugVariant? Find(string name) => Find(Encoding.ASCII.GetBytes(name));

    /// <inheritdoc />
    public override void Decode(Span<byte> buffer) => Process(buffer, this);

    private void Process(ReadOnlySpan<byte> buffer, Protocol.ProtocolLayer response)
    {
        if (buffer.Length == 0)
        {
            return;
        }

        char command = (char)buffer[0];
        switch (command)
        {
            case Debugger.CmdCapabilities:
            {
                // '?' -> <list of command chars>
                Span<byte> capabilities = Bytes("?rwelivamst");
                response.Encode(capabilities, true);
                return;
            }
            case Debugger.CmdRead:
            {
                // 'r' </path/to/object> -> <hex-encoded value>
                DebugVariant? variant = Find(buffer.Slice(1));
                if (variant == null)
                {
                    Debugger.SendNack(response);
                    return;
                }

                Span<byte> data = new byte[variant.Size];
                variant.CopyTo(data);
                Span<byte> hexData = Debugger.EncodeHex(data, variant.Type);
                response.Encode(hexData, true);
                return;
            }
            case Debugger.CmdWrite:
            {
                // 'w' <hex-encoded value> </path/to/object> -> '?' | '!'

                // find the '/' to split the data and path
                int slashIndex = buffer.IndexOf((byte)'/');
                if (slashIndex < 1)
                {
                    // Check if the last byte can be an alias.
                    if (buffer.Length - 1 != '/')
                    {
                        slashIndex = buffer.Length - 1;
                    }
                    else
                    {
                        Debugger.SendNack(response);
                        return;
                    }
                }

                ReadOnlySpan<byte> writeValue = buffer.Slice(1, slashIndex - 1);
                ReadOnlySpan<byte> path = buffer.Slice(slashIndex);
                DebugVariant? variant = Find(path);
                if (variant == null)
                {
                    Debugger.SendNack(response);
                    return;
                }

                // TODO: decode as hex-string
                bool ok = true;
                ReadOnlySpan<byte> decodedValue = DecodeHex(writeValue, variant.Type, ref ok);
                if (!ok || decodedValue.Length != variant.Size)
                {
                    Debugger.SendNack(response);
                    return;
                }

                variant.Set(decodedValue);

                break;
            }
            case Debugger.CmdEcho:
            {
                // 'e' <any data> -> <the data>
                response.Encode(buffer.Slice(1), true);
                return;
            }
            case Debugger.CmdList:
            {
                // 'l' -> ( <type byte in hex> <length in hex> <name of object> '\n' ) *
                List((s, dv) =>
                {
                    // TODO: use a more efficient way to encode the name and type, and store in array pool.

                    // Type
                    Span<byte> t = Debugger.EncodeHex([(byte)dv.Type], Types.Uint8, false);

                    // Length / size
                    byte[] sizeBytes = BitConverter.GetBytes(dv.Size);
                    Span<byte> l = Debugger.EncodeHex(sizeBytes, Types.Int32);

                    // Name
                    Span<byte> nameBytes = Encoding.ASCII.GetBytes(s);

                    // NewLine ('\n')
                    response.Encode([..t, ..l, ..nameBytes, (byte)'\n'], false);
                });

                response.Encode([], true);

                return;
            }
            case Debugger.CmdIdentification:
            {
                // 'i' -> <UTF-8 encoded string>
                if (_identification is null)
                {
                    Debugger.SendNack(response);
                    return;
                }

                byte[] bytes = Encoding.UTF8.GetBytes(_identification);
                response.Encode(bytes, true);

                return;
            }
            case Debugger.CmdVersion:
            {
                // 'v' -> <debugger version> ' ' <application-defined version>
                byte[] sizeBytes = BitConverter.GetBytes(Debugger._version);
                Span<byte> versionBuffer = Debugger.EncodeHex(sizeBytes, Types.Int32);
                response.Encode(versionBuffer, false);

                if (!string.IsNullOrEmpty(_versions))
                {
                    response.Encode([(byte)' '], false);
                    byte[] bytes = Encoding.UTF8.GetBytes(_versions);
                    response.Encode(bytes, false);
                }

                response.Encode([], true);

                return;
            }
            case Debugger.CmdAlias:
                // 'a' <char> </path/to/object? ?  -> '!' | '?'
                if (buffer.Length < 2)
                {
                    Debugger.SendNack(response);
                    return;
                }

                char a = (char)buffer[1];
                if (a < 0x20 || a > 0x7e || a == '/')
                {
                    Debugger.SendNack(response);
                    return;
                }

                if (buffer.Length == 2)
                {
                    // Remove alias
                    _aliases.Remove(a);
                    break;
                }

                DebugVariant? debugVariant = Find(buffer.Slice(2));
                if (debugVariant is null)
                {
                    Debugger.SendNack(response);
                    return;
                }

                // Dont limit the number of alias, max is limited to 255.
                // Add or replace.
                _aliases[a] = debugVariant;

                break;
            case Debugger.CmdMacro:
                // 'm' ( <char> ( <sep> <any command> ) * ? -> '!' | '?' | <bytes>

                if (buffer.Length == 1)
                {
                    // Returns all macros.
                    foreach ((char c, _) in _macros)
                    {
                        response.Encode([(byte)c], false);
                    }

                    response.Encode([], true);
                    return;
                }

                char m = (char)buffer[1];
                if (m == '?')
                {
                    // Invalid macro name
                    Debugger.SendNack(response);
                    return;
                }

                if (buffer.Length == 2)
                {
                    // Remove the macro
                    if (!_macros.TryGetValue(m, out byte[]? macro))
                    {
                        break;
                    }

                    if (macro.Length == 0)
                    {
                        // Macro in use
                        Debugger.SendNack(response);
                        return;
                    }

                    _macros.Remove(m);
                    break;
                }

                if (_macros.TryGetValue(m, out byte[]? currentMacro))
                {
                    // Update macro
                    int totalSize = buffer.Length - 2 - currentMacro.Length + _macros.Sum(x => x.Value.Length);
                    if (totalSize > _maxMacrosSize)
                    {
                        Debugger.SendNack(response);
                        return;
                    }

                    if (currentMacro.Length == 0)
                    {
                        // Macro in use
                        Debugger.SendNack(response);
                        return;
                    }

                    _macros[m] = buffer.Slice(2).ToArray();
                }
                else
                {
                    // Add new macro
                    int totalSize = buffer.Length - 2 + _macros.Sum(x => x.Value.Length);
                    if (totalSize > _maxMacrosSize)
                    {
                        Debugger.SendNack(response);
                        return;
                    }

                    _macros[m] = buffer.Slice(2).ToArray();
                }

                break;
            case Debugger.CmdReadMem:
            case Debugger.CmdWriteMem:
            case Debugger.CmdStream:
                // 's' ( <stream char> <suffix> ? ) ?
                // -> '?' | <stream char> +
                // -> <stream data> <suffix>

                if (buffer.Length == 1)
                {
                    if (_streams.Count == 0)
                    {
                        Debugger.SendNack(response);
                        return;
                    }

                    foreach ((char c, _) in _streams)
                    {
                        response.Encode([(byte)c], false);
                    }

                    response.Encode([], true);
                    return;
                }

                char s = (char)buffer[1];
                ReadOnlySpan<byte> suffix = buffer.Slice(2);

                Stream? stream = Stream(s);
                if (stream is null)
                {
                    Debugger.SendNack(response);
                    return;
                }

                List<byte> streamBuffer = [];
                stream.Swap(ref streamBuffer);
                Span<byte> streamBufferSpan = CollectionsMarshal.AsSpan(streamBuffer);
                response.Encode(streamBufferSpan, false);

                if (stream.Empty)
                {
                    // Clear and wwap back to avoid allocations.
                    streamBuffer.Clear();
                    stream.Swap(ref streamBuffer);
                }

                stream.Unblock();

                response.Encode(suffix, true);
                return;
            case Debugger.CmdFlush:
            case Debugger.CmdTrace:
                // Enable: 't' <macro> <stream> ( <decimation in dex>) ? -> '!' | '?'
                // Disable 't' -> '!' | '?'

                // Disable by default.
                _traceDecimation = 0;

                if (buffer.Length == 1)
                {
                    // Disable
                    break;
                }

                if (buffer.Length < 3)
                {
                    Debugger.SendNack(response);
                    return;
                }

                _traceMacro = (char)buffer[1];
                _traceStream = (char)buffer[2];

                if (Stream(_traceStream, true) is null)
                {
                    Debugger.SendNack(response);
                    return;
                }

                if (buffer.Length > 3)
                {
                    bool ok = true;
                    Span<byte> res = DecodeHex(buffer.Slice(3), Types.Uint64, ref ok);
                    if (!ok)
                    {
                        Debugger.SendNack(response);
                        return;
                    }
                    _traceDecimation = BinaryPrimitives.ReadUInt64LittleEndian(res);
                }
                else
                {
                    _traceDecimation = 1;
                }

                break;
            default:
                if (_macros.ContainsKey(command))
                {
                    if (RunMacro(command, response))
                    {
                        return;
                    }
                }

                Debugger.SendNack(response);
                return;
        }

        {
            response.Encode([(byte)Debugger.Ack], true);
        }
    }

    private static Span<byte> EncodeHex(Span<byte> data, Types type, bool shorten = true)
    {
        if (data.IsEmpty)
        {
            return Span<byte>.Empty;
        }

        int len = data.Length;

        if (type == Types.Bool)
        {
            Span<byte> boolResult = new byte[1];
            boolResult[0] = data[0] != 0 ? (byte)'1' : (byte)'0';
            return boolResult;
        }

        Span<byte> result = new byte[len * 2];

        bool swapEndian = true;

        // Fixed types are revered. Int types are also trimmed to remove leading '0's.
        if (type.IsFixed())
        {
            for (int i = 0; i < len; i++)
            {
                byte b = data[i];

                // Network byte order is big-endian.

                if (swapEndian)
                {
                    result[(len - i) * 2 - 2] = Debugger.EncodeNibble((byte)(b >> 4));
                    result[(len - i) * 2 - 1] = Debugger.EncodeNibble(b);
                }
                else
                {
                    result[i * 2] = Debugger.EncodeNibble((byte)(b >> 4));
                    result[i * 2 + 1] = Debugger.EncodeNibble(b);
                }
            }

            if (shorten && type.IsInt())
            {
                Span<byte> trimmed = result.TrimStart((byte)'0');
                if (trimmed.IsEmpty)
                {
                    // Make sure the result is at least 1 byte long
                    trimmed = result[^1..];
                }

                result = trimmed;
            }
        }
        else
        {
            for (int i = 0; i < len; i++)
            {
                byte b = data[i];
                result[i * 2] = Debugger.EncodeNibble((byte)(b >> 4));
                result[i * 2 + 1] = Debugger.EncodeNibble(b);
            }
        }

        //result[^1] = 0; // Null-terminate the string

        return result;
    }

    private Span<byte> DecodeHex(ReadOnlySpan<byte> data, Types type, ref bool ok)
    {
        if (data.IsEmpty)
        {
            ok = false;
            return Span<byte>.Empty;
        }

        ok = true;

        Span<byte> result;

        if (type.IsFixed())
        {
            int length = type.Size();
            if (length * 2 < data.Length)
            {
                ok = false;
                return Span<byte>.Empty;
            }

            bool swapEndian = true;

            result = new byte[length];
            for (int i = 0; i < data.Length; i++)
            {
                byte b = Debugger.DecodeNibble(data[data.Length - i - 1], ref ok);
                if ((i & 0b1) != 0)
                {
                    b = (byte)(b << 4);
                }

                if (swapEndian)
                {
                    result[i / 2] |= b;
                }
                else
                {
                    result[(data.Length - i - 1) / 2] |= b;
                }
            }
        }
        else
        {
            if ((data.Length & 0b1) != 0)
            {
                ok = false;
                return Span<byte>.Empty;
            }

            result = new byte[data.Length / 2];
            for (int i = 0; i < data.Length; i += 2)
            {
                byte highNibble = Debugger.DecodeNibble(data[i], ref ok);
                byte lowNibble = Debugger.DecodeNibble(data[i + 1], ref ok);
                result[i / 2] = (byte)(highNibble << 4 | lowNibble);
            }
        }

        return result;
    }

    private static byte EncodeNibble(byte n)
    {
        n &= 0xf;
        return (byte)((n < 10 ? (byte)'0' : 'a' - 10) + n);
    }

    private static byte DecodeNibble(byte n, ref bool ok)
    {
        if (n >= '0' && n <= '9')
        {
            return (byte)(n - '0');
        }

        if (n >= 'a' && n <= 'f')
        {
            return (byte)(n - ('a' - 10));
        }

        if (n >= 'A' && n <= 'F')
        {
            return (byte)(n - ('A' - 10));
        }

        ok = false;
        return 0;
    }

    private static void SendNack(Protocol.ProtocolLayer response) => response.Encode([(byte)Debugger.Nack], true);

    private DebugVariant? Find(ReadOnlySpan<byte> path)
    {
        if (_stores.Count == 0 || path.IsEmpty)
        {
            return null;
        }

        if (path.Length == 1 && path[0] != '/')
        {
            // Lookup alias
            char c = (char)path[0];
            if (_aliases.TryGetValue(c, out var alias))
            {
                return alias;
            }
        }

        if (_stores.Count == 1)
        {
            // If there is only one store, we can directly use it without prefix
            Store store = _stores.First().Value;
            DebugVariant? variant = store.Find(path);
            if (variant is not null)
            {
                return variant;
            }

            return store.Find(path.Slice(1));
        }

        string prefix = string.Empty;
        ReadOnlySpan<byte> remaining = ReadOnlySpan<byte>.Empty;
        if (path[0] == '/')
        {
            // Strip first / from name, and let the store eat the rest of the prefix.
            int indexPrefixEnd = path.Slice(1).IndexOf((byte)'/');
            if (indexPrefixEnd > 0)
            {
                // Use the part before the second '/' as prefix
                ReadOnlySpan<byte> prefixBytes = path.Slice(0, indexPrefixEnd + 1);
                remaining = path.Slice(indexPrefixEnd + 1);
                prefix = Encoding.ASCII.GetString(prefixBytes);
            }
            else
            {
                return null;
            }
        }

        // Rule 1 - exact match.
        if (_stores.TryGetValue(prefix, out Store? value))
        {
            return value.Find(remaining);
        }

        // Rule 2 - prefix match to only 1 store.
        if (_stores.Keys.Count(x => x.StartsWith(prefix)) == 1)
        {
            Store? store = _stores.First(x => x.Key.StartsWith(prefix)).Value;
            return store.Find(remaining);
        }

        return null;

    }

    private bool RunMacro(char macro, Protocol.ProtocolLayer response)
    {
        if (!_macros.TryGetValue(macro, out byte[]? macroBytes))
        {
            return false;
        }

        if (macroBytes.Length == 0)
        {
            // Macco is currently executing. Recursive calls are not allowed.
            return false;
        }
        else if (macroBytes.Length == 1)
        {
            // Nothing to execute. Only a separator.
            return true;
        }

        byte[] macroBytesLocal = macroBytes;
        _macros[macro] = [];

        byte sep = macroBytesLocal[0];

        FrameMerger merger = new(response);

        Span<byte> data = macroBytesLocal.AsSpan(1);
        do
        {
            int index = data.IndexOf(sep);
            ReadOnlySpan<byte> dataLocal = index < 0 ? data : data.Slice(0, index);
            Process(dataLocal, merger);
            data = index < 0 ? [] : data.Slice(index + 1);
        } while (!data.IsEmpty);

        response.Encode([], true);

        _macros[macro] = macroBytesLocal;

        return true;
    }

    /// <summary>
    /// Adds data to the stream. Tries to create it when it does not exists.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public int Stream(char s, ReadOnlySpan<byte> data)
    {
        if (_maxStreamCount < 1)
        {
            return 0;
        }

        if (s == '?')
        {
            return 0;
        }

        Stream? stream = Stream(s, true);
        if (stream is null)
        {
            return 0;
        }

        int len = stream.Fits(data.Length);
        if (len == 0)
        {
            return 0;
        }

        stream.Encode(data.Slice(0, len), true);
        return len;
    }

    /// <summary>
    /// Get the stream. May fail when all the maximum number of streams is already in use.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="alloc">Allocate when the stream does not yet exist.</param>
    /// <returns></returns>
    public Stream? Stream(char s, bool alloc = false)
    {
        if (_streams.TryGetValue(s, out Stream? stream))
        {
            return stream;
        }

        if (!alloc)
        {
            return null;
        }

        KeyValuePair<char, Stream> recycle = _streams.FirstOrDefault(x => x.Value.Empty);
        if (recycle.Value is not null && recycle.Value.Empty)
        {
            _streams.Remove(recycle.Key);
        }

        if (_streams.Count >= _maxStreamCount)
        {
            return null;
        }

        _streams[s] = stream = recycle.Value ?? new Stream();

        return stream;
    }

    /// <summary>
    /// Executes the trace macro and append the output to the trace stream.
    /// </summary>
    public void Trace()
    {
        if (_traceDecimation <= 0)
        {
            return;
        }

        if (++_traceCount < _traceDecimation)
        {
            return;
        }

        _traceCount = 0;

        Stream? stream = Stream(_traceStream, true);
        if (stream is null)
        {
            return;
        }

        if (stream.IsFull)
        {
            return;
        }

        RunMacro(_traceMacro, stream);
    }

    private byte[] Bytes(string text) => Encoding.ASCII.GetBytes(text);
}
