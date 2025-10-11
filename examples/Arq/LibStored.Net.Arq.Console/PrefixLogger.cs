// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;
using LibStored.Net;
using LibStored.Net.Protocol;
using Microsoft.Extensions.Logging;

class PrefixLogger : ProtocolLayer
{
    private readonly ILogger<PrefixLogger> _logger;

    /// <inheritdoc />
    public PrefixLogger(ILogger<PrefixLogger> logger) 
    {
        _logger = logger;
    }

    public string Prefix { get; set; } = "";

    /// <summary>
    /// Logs the buffer and passes it to the next layer for decoding.
    /// </summary>
    /// <param name="buffer">The buffer to decode.</param>
    public override void Decode(Span<byte> buffer)
    {
        Log(buffer, $"{Prefix}Decoding:");
        base.Decode(buffer);
    }

    /// <summary>
    /// Logs the buffer and passes it to the next layer for encoding.
    /// </summary>
    /// <param name="buffer">The buffer to encode.</param>
    /// <param name="last">Indicates if this is the last buffer in the message.</param>
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        Log(buffer, $"{Prefix}Encoding:");
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
