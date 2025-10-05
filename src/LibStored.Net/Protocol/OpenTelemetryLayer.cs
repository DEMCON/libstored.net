// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace LibStored.Net.Protocol;

/// <summary>
/// Adds OpenTelemetry support to the protocol stack.
/// Provides tracing (Activities) and metrics (Counters) for observability.
/// </summary>
/// <remarks>
/// Counters exposed:
///  - layer.encode
///  - layer.decode
///  - layer.flush
///  - layer.reset
///
/// The <see cref="ActivitySourceName"/> and <see cref="MeterName"/> constants are used when wiring OpenTelemetry.
///
/// <para>
/// Typical configuration using the helper extension (recommended):
/// </para>
/// <example>
/// <code>
/// // In your DI setup (e.g. Program.cs / Host builder):
/// builder.Services.AddLibStoredProtocolLayers();
/// </code>
/// </example>
///
/// <para>
/// Configuration OpenTelemetry traces and metrics:
/// </para>
/// <example>
/// <code>
/// builder.Services.AddOpenTelemetry()
///     .WithTracing(b =&gt; b.AddSource(OpenTelemetryLayer.ActivitySourceName))
///     .WithMetrics(b =&gt; b.AddMeter(OpenTelemetryLayer.MeterName));
/// </code>
/// </example>
///
/// <para>
/// Once configured, any stack including <see cref="OpenTelemetryLayer"/> will emit Activities named:
///  - "Encode"
///  - "Decode"
///  - "Flush"
///  - "Reset"
/// </para>
/// </remarks>
public class OpenTelemetryLayer : ProtocolLayer
{
    /// <summary>
    /// Name of the <see cref="s_activitySource"/> used for tracing.
    /// Pass this to OpenTelemetry tracing configuration (e.g. AddSource(ActivitySourceName)).
    /// </summary>
    public const string ActivitySourceName = "LibStored.Net";

    /// <summary>
    /// Name of the <see cref="s_meter"/> used for metrics.
    /// Pass this to OpenTelemetry metrics configuration (e.g. AddMeter(MeterName)).
    /// </summary>
    public const string MeterName = "LibStored.Net";

    /// <summary>
    /// Semantic version applied to the instrumentation (ActivitySource + Meter).
    /// </summary>
    public const string Version = "0.1.0";

    private static readonly ActivitySource s_activitySource = new(ActivitySourceName, Version);
    private static readonly Meter s_meter = new(MeterName, Version);

    private static readonly Counter<long> s_encodeCounter =
        s_meter.CreateCounter<long>("layer.encode", description: "Counts the number of messages sent");
    private static readonly Counter<long> s_decodeCounter =
        s_meter.CreateCounter<long>("layer.decode", description: "Counts the number of messages received");
    private static readonly Counter<long> s_flushCounter =
        s_meter.CreateCounter<long>("layer.flush", description: "Counts Flush() invocations");
    private static readonly Counter<long> s_resetCounter =
        s_meter.CreateCounter<long>("layer.reset", description: "Counts Reset() invocations");

    /// <summary>
    /// Records a receive operation by starting a consumer <see cref="Activity"/> (if listeners are present),
    /// tagging it with the payload length, incrementing the received counter and passing the buffer upward.
    /// </summary>
    /// <param name="buffer">The decoded bytes for this fragment/message.</param>
    public override void Decode(Span<byte> buffer)
    {
        using Activity? activity = s_activitySource.StartActivity("Decode", ActivityKind.Consumer);
        activity?.AddTag("Length", buffer.Length);
        s_decodeCounter.Add(1);
        base.Decode(buffer);
    }

    /// <summary>
    /// Records a send operation by starting a producer <see cref="Activity"/> (if listeners are present),
    /// tagging it with the payload length, incrementing the sent counter and passing the buffer downward.
    /// </summary>
    /// <param name="buffer">The data being encoded / transmitted.</param>
    /// <param name="last">Indicates whether this is the final fragment of the logical message.</param>
    public override void Encode(ReadOnlySpan<byte> buffer, bool last)
    {
        using Activity? activity = s_activitySource.StartActivity("Encode", ActivityKind.Producer);
        activity?.AddTag("Length", buffer.Length);
        s_encodeCounter.Add(1);
        base.Encode(buffer, last);
    }

    /// <summary>
    /// Flushes buffered data in lower layers while recording a tracing Activity (Internal kind)
    /// and incrementing a flush counter. The return value is propagated unchanged.
    /// </summary>
    /// <returns>True if lower layers flushed data; otherwise false.</returns>
    public override bool Flush()
    {
        using Activity? activity = s_activitySource.StartActivity("Flush", ActivityKind.Internal);
        bool result = base.Flush();
        activity?.AddTag("Flushed", result);
        s_flushCounter.Add(1);
        return result;
    }

    /// <summary>
    /// Resets this layer and all lower layers, recording a tracing Activity and incrementing a reset counter.
    /// </summary>
    public override void Reset()
    {
        using Activity? _ = s_activitySource.StartActivity("Reset", ActivityKind.Internal);
        s_resetCounter.Add(1);
        base.Reset();
    }
}
