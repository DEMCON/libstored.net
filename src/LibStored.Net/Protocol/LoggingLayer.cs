// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;

namespace LibStored.Net.Protocol;

public class LoggingLayer : ProtocolLayer
{
    public List<string> Decoded = [];
    public List<string> Encoded = [];

    private bool _partial;

    public override void Decode(Span<byte> buffer)
    {
        string decodedData = Encoding.Latin1.GetString(buffer);
        Decoded.Add(decodedData);
        base.Decode(buffer);
    }

    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        string encodedData = Encoding.Latin1.GetString(buffer);

        //string encodedData = Encoding.UTF8.GetString(buffer);
        //string encodedData = Encoding.ASCII.GetString(buffer);

        if (_partial && Encoded.Count > 0)
        {
            Encoded[^1] += encodedData; // Append to the last entry if this is a partial message
        }
        else
        {
            Encoded.Add(encodedData);
        }

        _partial = !last;

        base.Encode(buffer, last);
    }

    public void Clear()
    {
        Decoded.Clear();
        Encoded.Clear();
        _partial = false;
    }
}