// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that logs all encoded and decoded data as strings for inspection and testing.
/// Stores the encoded and decoded messages in lists for later retrieval.
/// </summary>
public class LoggingLayer : ProtocolLayer
{
    /// <summary>
    /// Gets the list of decoded messages as strings.
    /// </summary>
    public List<string> Decoded = [];
    
    /// <summary>
    /// Gets the list of encoded messages as strings.
    /// </summary>
    public List<string> Encoded = [];

    private bool _partial;

    /// <summary>
    /// Decodes the buffer, logs the decoded string, and passes it to the next layer.
    /// </summary>
    /// <param name="buffer">The buffer to decode.</param>
    public override void Decode(Span<byte> buffer)
    {
        string decodedData = Encoding.Latin1.GetString(buffer);
        Decoded.Add(decodedData);
        base.Decode(buffer);
    }

    /// <summary>
    /// Encodes the buffer, logs the encoded string, and passes it to the next layer.
    /// </summary>
    /// <param name="buffer">The buffer to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
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

    /// <summary>
    /// Clears all logged encoded and decoded messages and resets the partial state.
    /// </summary>
    public void Clear()
    {
        Decoded.Clear();
        Encoded.Clear();
        _partial = false;
    }
}
