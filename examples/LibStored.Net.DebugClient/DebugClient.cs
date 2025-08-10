// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using System.Text;
using LibStored.Net.Protocol;
using LibStored.Net.ZeroMQ;

namespace LibStored.Net.DebugClient;

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

    public string[] List()
    {
        SendCmd('l');
        string text = ReceiveString();
        return text.Split('\n');
    }

    public string Echo(string text)
    {
        SendCmd('e', Encoding.ASCII.GetBytes(text));
        return ReceiveString();
    }

    /// <summary>
    /// Returns the value as Hex string.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string Read(string path)
    {
        SendCmd('r', Encoding.ASCII.GetBytes(path));
        return ReceiveString();
    }

    public override void Decode(Span<byte> buffer)
    {
        _receiveBuffer.AddRange(buffer);
        base.Decode(buffer);
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
}
