// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.Extensions.Logging;

namespace LibStored.Net.Protocol;

/// <summary>
/// A protocol layer that logs encoded and decoded data for debugging and diagnostics.
/// </summary>
public class LoggerLayer : ProtocolLayer
{
    private readonly ILogger<LoggerLayer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggerLayer"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for debug output.</param>
    public LoggerLayer(ILogger<LoggerLayer> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs the buffer and passes it to the next layer for decoding.
    /// </summary>
    /// <param name="buffer">The buffer to decode.</param>
    public override void Decode(Span<byte> buffer)
    {
        Log(buffer, "Decoding:");
        base.Decode(buffer);
    }

    /// <summary>
    /// Logs the buffer and passes it to the next layer for encoding.
    /// </summary>
    /// <param name="buffer">The buffer to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        Log(buffer, "Encoding:");
        base.Encode(buffer, last);
    }

    private void Log(ReadOnlySpan<byte> buffer, string prefix)
    {
        string bytes = BitConverter.ToString(buffer.ToArray());
        string text = Encoding.ASCII.GetString(buffer);
        string literalText = StringUtils.StringLiteral(text);
        int size = buffer.Length;
        _logger.LogDebug("{Prefix} [{Size}] {Literal} [{Bytes}]", prefix, size, literalText, bytes);
    }
}
