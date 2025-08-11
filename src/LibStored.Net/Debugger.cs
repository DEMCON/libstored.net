// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;

namespace LibStored.Net;

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

    private readonly Dictionary<string, Store> _stores = [];

    private string? _identification;
    private string _versions;
    private const int _version = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="Debugger"/> class.
    /// </summary>
    /// <param name="identification">Optional identification string for the debugger.</param>
    /// <param name="versions">Optional version string for the application.</param>
    public Debugger(string? identification = null, string versions = "")
    {
        _identification = identification;
        _versions = versions;
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
                Span<byte> capabilities = Bytes("?rweliv");
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
                    Debugger.SendNack(response);
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
            case Debugger.CmdMacro:

            case Debugger.CmdReadMem:
            case Debugger.CmdWriteMem:
            case Debugger.CmdStream:
            case Debugger.CmdTrace:
            case Debugger.CmdFlush:
            default:
                Debugger.SendNack(response);
                return;
        }

        {
            response.Encode([(byte)Debugger.Ack], true);
        }
    }

    private static Span<byte> EncodeHex2(Span<byte> data)
    {
        Span<byte> reversed = stackalloc byte[data.Length];
        data.CopyTo(reversed);
        reversed = reversed.TrimEnd((byte)0b0);
        if (!BitConverter.IsLittleEndian)
        {
            //BinaryPrimitives.ReverseEndianness();
            reversed.Reverse(); // Reverse the byte order to match network byte order (big-endian)
        }

        // Try to convert the reversed bytes to a hex bytes, efficiently
        // This is a more efficient way to convert bytes to hex without using string operations
        // It uses stackalloc for temporary storage and avoids unnecessary allocations
        Span<char> hexChars = stackalloc char[reversed.Length * 2];
        if (!Convert.TryToHexStringLower(reversed, hexChars, out int written))
        {
            throw new InvalidOperationException("Failed to encode hex string");
        }

        hexChars = hexChars.Slice(0, written);

        // Cant stackalloc byte[] here since this will be the result, so we use a new byte array
        Span<byte> res = new byte[hexChars.Length];
        int resWritten = Encoding.UTF8.GetBytes(hexChars, res);
        res = res.Slice(0, resWritten);

        // Convert the hex characters to bytes, not that efficiently
        string hex = Convert.ToHexStringLower(reversed);
        Span<byte> hexBytes = Encoding.UTF8.GetBytes(hex);
        return hexBytes;
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
            // Remove
            Debugger.EncodeHex2(data);

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

        if (_stores.Count == 1)
        {
            // If there is only one store, we can directly use it without prefix
            Store store = _stores.First().Value;
            DebugVariant? variant =  store.Find(path);
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

    private byte[] Bytes(string text) => Encoding.ASCII.GetBytes(text);
}
