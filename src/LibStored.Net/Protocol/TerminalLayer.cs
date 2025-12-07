// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that wraps messages with terminal escape sequences and logs terminal messages.
/// Handles segmentation, message boundaries, and non-debug data output.
/// </summary>
public class TerminalLayer : ProtocolLayer
{
    private static readonly byte[] Start = [0x1b, 0x5f]; // Application Program Command (ESC _)
    private static readonly byte[] End = [0x1b, 0x5c]; // String Terminator (ESC \)

    private readonly ILogger<TerminalLayer> _logger;

    private bool _ignoreEscape = false;
    private List<byte> _data = [];
    private bool _decodingMessage;
    private bool _encodingMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="TerminalLayer"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for terminal messages.</param>
    public TerminalLayer(ILogger<TerminalLayer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public override void Decode(Span<byte> buffer)
    {
        // TODO: look at cpp implementation using more efficient states.
        if (buffer.IsEmpty)
        {
            return; // Nothing to decode
        }

        if (_ignoreEscape && !_decodingMessage)
        {
            NonDebugData(buffer);
            return;
        }

        _data.AddRange(buffer);

        if (buffer[^1] == TerminalLayer.Start[0])
        {
            // Partial escape code. Wait for more data.
            return;
        }

        while (true)
        {
            ReadOnlySpan<byte> span = CollectionsMarshal.AsSpan(_data);
            if (!_decodingMessage)
            {
                int startIndex = span.IndexOf(TerminalLayer.Start);
                if (startIndex >= 0)
                {
                    // Found the start sequence, output any data before it
                    NonDebugData(span.Slice(0, startIndex));
                }
                else
                {
                    // No start sequence found
                    // Send the remaining non-debug data.
                    NonDebugData(span);

                    _data.Clear();
                    return;
                }

                _data = [.. span.Slice(startIndex + TerminalLayer.Start.Length)];
                _decodingMessage = true; // Set the message state
            }
            else
            {
                int endIndex = span.IndexOf(TerminalLayer.End);
                if (endIndex < 0)
                {
                    // No end sequence found, wait for more data
                    return;
                }

                // Found the end sequence, output the message
                Span<byte> message = new byte[endIndex];
                span.Slice(0, endIndex).CopyTo(message);

                // Remove carriage return ('\r' if present)
                Span<byte> cleanedMessage = TerminalLayer.Remove(message, 0x0d);

                _data = [.. span.Slice(endIndex + TerminalLayer.End.Length)];
                _decodingMessage = false; // Reset the message state
                base.Decode(cleanedMessage);
            }
        }
    }

    /// <inheritdoc />
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        if (!_encodingMessage)
        {
            // Add start sequence if not already encoding a message
            base.Encode(TerminalLayer.Start, false);
            _encodingMessage = true;
        }

        _ignoreEscape = false;
        base.Encode(buffer, false);

        if (last)
        {
            // Add end sequence if this is the last part of the message
            base.Encode(TerminalLayer.End, true);
            _encodingMessage = false; // Reset encoding state
        }
    }

    /// <inheritdoc />
    public override void Reset()
    {
        _data.Clear();
        _decodingMessage = false;
        _encodingMessage = false;
        base.Reset();
    }

    /// <inheritdoc />
    public override void Disconnected()
    {
        _data.Clear();
        _decodingMessage = false;
        base.Disconnected();
    }

    /// <inheritdoc />
    public override int Mtu() => base.Mtu() switch
    {
        0 => 0, // No limit
        <= 4 => 1,
        var x => x - 4,
    };

    private static Span<byte> Remove(Span<byte> span, byte value)
    {
        int count = 0;
        for (int i = 0; i < span.Length; i++)
        {
            if (span[i] != value)
            {
                span[count++] = span[i];
            }
        }

        return span.Slice(0, count);
    }

    /// <summary>
    /// Handles non-debug data by logging and writing it to the console output.
    /// </summary>
    /// <param name="data">The non-debug data to output.</param>
    protected virtual void NonDebugData(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return;
        }

        string output = Encoding.ASCII.GetString(data);
        _logger.LogInformation("Non-debug data: {Data}", output);
        Console.Out.Write(output);
    }
}
