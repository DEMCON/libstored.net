// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using LibStored.Net;
using LibStored.Net.Arq.ServiceDefaults;
using LibStored.Net.Protocol;
using LibStored.Net.Synchronization;
using LibStored.Net.ZeroMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddOpenTelemetry()
    .WithTracing(x => x.AddSource(OpenTelemetryLayer.ActivitySourceName))
    .WithMetrics(x => x.AddMeter(OpenTelemetryLayer.MeterName))
    ;

builder.Services.AddTransient<PrefixLogger>();
builder.Services.AddLibStoredProtocolLayers();
builder.Services.AddHostedService<App>();

try
{
    await builder.Build().RunAsync();
}
finally
{
    NetMQConfig.Cleanup();
}

internal class App : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<App> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IServiceProvider _services;

    public App(ILogger<App> logger, IConfiguration configuration, ILoggerFactory loggerFactory, IServiceProvider services)
    {
        _logger = logger;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _services = services;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        ArqStore store = new();
        SynchronizableStore<ArqStore> syncStore = new(store);
        Synchronizer synchronizer = new();
        synchronizer.Map(syncStore);

        ILogger storeLogger = _loggerFactory.CreateLogger(nameof(ArqStore));
        store.PropertyChanged += (_, args) =>
        {
            string value = args.PropertyName switch
            {
                nameof(ArqStore.Ber) => store.Ber.ToString(CultureInfo.InvariantCulture),
                nameof(ArqStore.MTU) => store.MTU.ToString(),
                nameof(ArqStore.ALongStringThatDoesNotFitInTheResponseBuffers) => store.ALongStringThatDoesNotFitInTheResponseBuffers,
                nameof(ArqStore.World) => store.World.ToString(CultureInfo.InvariantCulture),
                nameof(ArqStore.Hello) => store.Hello.ToString(),
                nameof(ArqStore.AShortIntThatFitsInTheResponseBuffers) => store.AShortIntThatFitsInTheResponseBuffers.ToString(),
                nameof(ArqStore.InjectedErrors) => store.InjectedErrors.ToString(),
                _ => string.Empty
            };

            storeLogger.LogInformation("Changed {Property} - {Value}", args.PropertyName, value);
        };

        string connectionString = _configuration["NETMQ_CLIENT"] ?? throw new Exception("NETMQ_CLIENT not set");

        DealerSocket socket = new(connectionString);

        ProtocolBuilder.ProtocolStack syncStack = ProtocolBuilder.Create(_services)
            .Add(synchronizer)
            .Add<BufferLayer>()
            .Add<SegmentationLayer>()
            .Add<LoggerLayer>()
            .Add<ArqLayer>()
            .Add<PrefixLogger>(x => x.Prefix = "Arq")
            .Add<Crc16Layer>()
            .Add<AsciiEscapeLayer>()
            .Add<TerminalLayer>()
            .Add<BufferLayer>()
            .Add<PrefixLogger>(x => x.Prefix = "Zmq")
            .Add<OpenTelemetryLayer>()
            .Add(new SyncZeroMQLayer(socket))
            .Build();

        Console.WriteLine(syncStack.ToAsciiArt());

        SyncConnection syncLayer = (syncStack.Layers.First() as SyncConnection)!;
        SyncZeroMQLayer syncZmqLayer = (syncStack.Layers.Last() as SyncZeroMQLayer)!;

        ArqLayer arqLayer = (ArqLayer)syncStack.Layers.First(x => x is ArqLayer);
        bool connected = false;
        arqLayer.EventOccurred += (_, e) =>
        {
            _logger.LogInformation("Arq Event: {Event}", e.Event);
            if (e.Event is ArqEvent.Connected or ArqEvent.Reconnect)
            {
                connected = true;
            }
            else if (e.Event == ArqEvent.Retransmit)
            {
                connected = false;
            }
        };

        synchronizer.SyncFrom(syncStore, syncLayer);

        NetMQTimer syncTimer = new(TimeSpan.FromMilliseconds(1000));
        NetMQTimer keepAliveTimer = new(TimeSpan.FromMilliseconds(5000));
        using (NetMQPoller poller = [syncTimer, keepAliveTimer, socket])
        {
            socket.ReceiveReady += (_, _) =>
            {
                int bytesReceived = syncZmqLayer.ReceiveAll();
                _logger.LogTrace("Sync Received {Bytes} bytes", bytesReceived);
            };

            keepAliveTimer.Elapsed += (_, _) =>
            {
                arqLayer.KeepAlive();
            };

            syncTimer.Elapsed += (_, _) =>
            {
                if (connected)
                {
                    store.Hello += 1;

                    if (store.Hello == 10)
                    {
	                    store.Ber = 1e-3;
                    }
                }
                synchronizer.Process();
            };

            // ReSharper disable once MethodHasAsyncOverload
            poller.Run();
        }

        socket.Dispose();
    }
}
