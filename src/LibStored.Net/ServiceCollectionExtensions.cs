// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using LibStored.Net.Protocol;
using LibStored.Net.Debugging;

namespace LibStored.Net;

/// <summary>
/// Registers all built‑in protocol layers as transient services.
/// No reflection; each layer is explicitly added.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all protocol layers with a simple transient lifetime so multiple stacks
    /// can be composed independently. Layers that need constructor params have defaults.
    /// </summary>
    public static IServiceCollection AddLibStoredProtocolLayers(this IServiceCollection services)
    {
        // Application layers
        services.AddTransient<Debugger>();
        // The SynchronizationConnection needs to be created from a Synchronizer; not auto-registered.

        // Stateless / simple
        services.AddTransient<AsciiEscapeLayer>();
        services.AddTransient<BufferLayer>();
        services.AddTransient<LoggingLayer>();
        services.AddTransient<OpenTelemetryLayer>();
        services.AddTransient<TerminalLayer>();

        services.AddTransient<LoggerLayer>();
        services.AddTransient<Crc8Layer>();
        services.AddTransient<Crc16Layer>();

        // Parameterized layers (use default ctor values)
        services.AddTransient<SegmentationLayer>();     // default mtu
        services.AddTransient<ArqLayer>();              // default maxEncodeBufferSize

        // LoopbackLayer requires two peer layers; not auto-registered (needs runtime pair wiring).

        return services;
    }
}
