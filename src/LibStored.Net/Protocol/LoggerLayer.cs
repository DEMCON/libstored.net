// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.Extensions.Logging;

namespace LibStored.Net.Protocol;

public class LoggerLayer : ProtocolLayer
{
    private readonly ILogger<LoggerLayer> _logger;

    public LoggerLayer(ILogger<LoggerLayer> logger)
    {
        _logger = logger;
    }

    public override void Decode(Span<byte> buffer)
    {
        Log(buffer, "Decoding:");
        base.Decode(buffer);
    }

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