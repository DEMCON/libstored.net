// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Globalization;
using LibStored.Net;
using LibStored.Net.Example.Console;
using LibStored.Net.Example.ServiceDefaults;
using LibStored.Net.Protocol;
using LibStored.Net.Synchronization;
using LibStored.Net.ZeroMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;

HostApplicationBuilder builder = Host.CreateApplicationBuilder();

builder.AddServiceDefaults();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddTransient<Debugger>();
builder.Services.AddTransient<LoggerLayer>();
builder.Services.AddTransient<BufferLayer>();


builder.Services.AddHostedService<App>();

try
{
    await builder.Build().RunAsync();
}
finally
{
    NetMQConfig.Cleanup();
}


namespace LibStored.Net.Example.Console
{
    class App : BackgroundService
    {
        private static TimeSpan IncrementInterval = TimeSpan.FromSeconds(1);
        private static TimeSpan SyncInterval = TimeSpan.FromSeconds(0.1);

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
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{Name} Started", nameof(App));

            bool useBuilder = true;
            string? connectionStringServer = _configuration["NETMQ_SERVER"];
            string? connectionStringClient = _configuration["NETMQ_CLIENT"];
            bool useDebug = int.TryParse(_configuration["NETMQ_DEBUG"], out int debugPort);

            SynchronizableStore<ExampleSync1> store = new(new ExampleSync1());

            store.Store().PropertyChanged += (_, args) =>
            {
                string value = args.PropertyName switch
                {
                    nameof(ExampleSync1.I) => store.Store().I.Get().ToString(),
                    nameof(ExampleSync1.D) => store.Store().D.Get().ToString(CultureInfo.InvariantCulture),
                    _ => string.Empty,
                };
                _logger.LogInformation("Property changed: {PropertyName} = {Value}", args.PropertyName, value);
            };

            List<NetMQSocket> sockets = [];

            // Create the stack for the Debugger
            if (useDebug)
            {
                ResponseSocket debugSocket = new($"@tcp://*:{debugPort}");
                sockets.Add(debugSocket); 

                ProtocolBuilder.ProtocolStack stack = CreateDebugStack(debugSocket, store.Store(), useBuilder);

                DebugZeroMQLayer debugLayer = stack.Layers.Last() as DebugZeroMQLayer ?? throw new Exception("Bottom should be a DebugZeroMQLayer");

                debugSocket.ReceiveReady += (_, _) =>
                {
                    int bytesReceived = debugLayer.ReceiveAll();
                    _logger.LogTrace("Debug Received {Bytes} bytes", bytesReceived);
                };

                System.Console.WriteLine(stack.ToAsciiArt());
            }

            Synchronizer synchronizer = new();
            synchronizer.Map(store);

            // Create the stack for the Synchronizer
            if (connectionStringServer is not null)
            {
                DealerSocket socket = new(connectionStringServer);
                sockets.Add(socket);

                ProtocolBuilder.ProtocolStack syncStack = CreateSyncStack(socket, synchronizer, useBuilder);
                System.Console.WriteLine(syncStack.ToAsciiArt());

                // Not sure why we actually need to layer below the syncConnectionLayer...
                SyncZeroMQLayer syncLayer = (syncStack.Layers.Last() as SyncZeroMQLayer)!;
                
                socket.ReceiveReady += (_, _) =>
                {
                    int bytesReceived = syncLayer.ReceiveAll();
                    _logger.LogTrace("Sync Received {Bytes} bytes", bytesReceived);
                };
            }

            if (connectionStringClient is not null)
            {
                DealerSocket socket = new(connectionStringClient);
                sockets.Add(socket);
            
                ProtocolBuilder.ProtocolStack syncStack = CreateSyncStack(socket, synchronizer, useBuilder);
                System.Console.WriteLine(syncStack.ToAsciiArt());

                // Not sure why we actually need the layer below the syncConnectionLayer...
                SyncConnection syncConnectionLayer = (syncStack.Layers.First() as SyncConnection)!;
                SyncZeroMQLayer syncLayer = (syncStack.Layers.Last() as SyncZeroMQLayer)!;
            
                synchronizer.SyncFrom(store, syncConnectionLayer);

                socket.ReceiveReady += (_, _) =>
                {
                    int bytesReceived = syncLayer.ReceiveAll();
                    _logger.LogTrace("Sync Received {Bytes} bytes", bytesReceived);

                    int number = store.Store().I.Get();
                    _logger.LogInformation("I: {Number}", number);
                };
            }

        
            NetMQTimer incrementTimer = new(App.IncrementInterval);
            NetMQTimer syncTimer = new(App.SyncInterval);
            using (NetMQPoller poller = [incrementTimer, syncTimer, ..sockets])
            {
                stoppingToken.Register(poller.StopAsync);

                incrementTimer.Elapsed += (_, _) =>
                {
                    // Only the server should increment the number
                    if (connectionStringServer is not null && connectionStringClient is null)
                    {
                        int number = store.Store().I.Get();
                        int newNumber = number + 1;
                        store.Store().I.Set(newNumber);
                        _logger.LogInformation("I set to: {Number}", newNumber);
                    }

                    // Update from the middle node, so that both server and client can increment the fraction.
                    if (connectionStringServer is not null && connectionStringClient is not null)
                    {
                        // increment from the middle node.
                        double fraction = store.Store().D.Get();
                        fraction++;
                        store.Store().D.Set(fraction);
                        _logger.LogInformation("D set to: {Number}", fraction);
                    }
                };

                syncTimer.Elapsed += (_, _) =>
                {
                    synchronizer.Process();
                };

                // ReSharper disable once MethodHasAsyncOverload
                poller.Run();
            }

            foreach (NetMQSocket socket in sockets)
            {
                socket.Dispose();
            }

            return Task.CompletedTask;
        }

        private ProtocolBuilder.ProtocolStack CreateDebugStack(ResponseSocket debugSocket, Store store, bool useBuilder)
        {
            ProtocolBuilder.ProtocolStack stack;

            if (useBuilder)
            {
                stack = ProtocolBuilder.Create(_services)
                    .Add<Debugger>(x =>
                    {
                        x.Identification = "8_sync";
                        x.Versions = "123";
                        x.Map(store);
                    })
                    .Add<BufferLayer>()
                    .Add<LoggerLayer>()
                    .Add<DebugZeroMQLayer>(new DebugZeroMQLayer(debugSocket))
                    .Build();
            }
            else
            {
                Debugger debugger = new("8_sync", "123");
                debugger.Map(store);

                BufferLayer buffer = new();
                buffer.Wrap(debugger);

                LoggerLayer debugLogging = new(_loggerFactory.CreateLogger<LoggerLayer>());
                debugLogging.Wrap(buffer);

                DebugZeroMQLayer debugLayer = new DebugZeroMQLayer(debugSocket);
                debugLayer.Wrap(debugLogging);

                stack = ProtocolBuilder.ProtocolStack.Traverse(debugger);
            }
            return stack;
        }

        private ProtocolBuilder.ProtocolStack CreateSyncStack(DealerSocket socket, Synchronizer synchronizer, bool useBuilder)
        {
            ProtocolBuilder.ProtocolStack syncStack;
            if (useBuilder)
            {
                syncStack = ProtocolBuilder.Create(_services)
                    .Add<SyncConnection>(synchronizer.CreateConnectionLayer())
                    .Add<BufferLayer>()
                    .Add<LoggerLayer>()
                    .Add<SyncZeroMQLayer>(new SyncZeroMQLayer(socket))
                    .Build();
            }
            else
            {
                BufferLayer buffer = new();

                LoggerLayer logging = new(_loggerFactory.CreateLogger<LoggerLayer>());
                logging.Wrap(buffer);

                SyncZeroMQLayer sync = new(socket);
                sync.Wrap(logging);

                synchronizer.Connect(buffer);

                syncStack = ProtocolBuilder.ProtocolStack.Traverse(sync);
            }

            return syncStack;
        }
    }
}