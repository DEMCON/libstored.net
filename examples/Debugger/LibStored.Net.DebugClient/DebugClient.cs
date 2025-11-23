// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using System.Text;
using LibStored.Net.Protocol;
using LibStored.Net.ZeroMQ;

namespace LibStored.Net.DebugClient;

public record Register(Types Type, int Size, string Path);

public class DebugClient : ProtocolLayer
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    private readonly ZeroMQLayer _layer;
    private readonly List<byte> _receiveBuffer = [];

    public DebugClient(ZeroMQLayer layer)
    {
        _layer = layer;
        _layer.Wrap(this);
    }

    public string Capabilities()
    {
        SendCmd('?');
        return ReceiveString();
    }

    public Register[] List()
    {
        SendCmd('l');
        string text = ReceiveString();
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => ParseRegister(x))
            .ToArray();
    }

    public string Echo(string text)
    {
        SendCmd('e', Encoding.ASCII.GetBytes(text));
        return ReceiveString();
    }

    public object Read(Register register)
    {
        string hexValue = Read(register.Path);
        string extendedHexValue = hexValue.PadLeft(register.Size * 2, '0');
        byte[] bytes = Convert.FromHexString(extendedHexValue);
        object value = TypesExtensions.ReadValue(bytes, register.Type, register.Size, bigEndian: true);
        return value;
    }

    public bool Write(Register register, object value)
    {
        Span<byte> writeBytes = new byte[register.Size];
        TypesExtensions.WriteValue(value, writeBytes, register.Type, register.Size, bigEndian: true);
        string writeHex = Convert.ToHexString(writeBytes);
        string writeHexShort = writeHex.TrimStart('0');
        // Send at least 1 char.
        writeHexShort = writeHexShort.Length > 0 ? writeHexShort : writeHex[^1].ToString();
        bool success = Write(register.Path, writeHexShort);
        return success;
    }

    public override void Decode(Span<byte> buffer)
    {
        _receiveBuffer.AddRange(buffer);
        base.Decode(buffer);
    }

    /// <summary>
    /// Returns the value as Hex string.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private string Read(string path)
    {
        SendCmd('r', Encoding.ASCII.GetBytes(path));
        return ReceiveString();
    }

    /// <summary>
    /// Returns the value as Hex string.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="hex"></param>
    /// <returns></returns>
    private bool Write(string path, string hex)
    {
        SendCmd('w', [..Encoding.ASCII.GetBytes(hex), ..Encoding.ASCII.GetBytes(path)]);
        return ReceiveString() == "!";
    }

    private void SendCmd(char cmd, params byte[] data)
    {
        // Use stackalloc, since we send little data.
        Span<byte> buffer = stackalloc byte[data.Length + 1];
        buffer[0] = (byte)cmd;
        data.CopyTo(buffer.Slice(1));
        Encode(buffer, true);
    }

    private string ReceiveString()
    {
        int receivedBytes = _layer.ReceiveAll(Timeout);
        var span = CollectionsMarshal.AsSpan(_receiveBuffer);
        string text = Encoding.ASCII.GetString(span);
        _receiveBuffer.Clear();
        return text;
    }

    private Register ParseRegister(ReadOnlySpan<char> line)
    {
        ReadOnlySpan<char> typeStr = line.Slice(0, 2);
        int pathIndex = line.IndexOf('/');
        ReadOnlySpan<char> sizeStr = line.Slice(2, pathIndex - 2);
        ReadOnlySpan<char> pathStr = line.Slice(pathIndex);

        Types t = TypesExtensions.Parse(typeStr);
        int size = int.Parse(sizeStr);
        return new Register(t, size, pathStr.ToString());
    }
}
