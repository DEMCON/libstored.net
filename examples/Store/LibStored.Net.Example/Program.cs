// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

using System.Text;
using LibStored.Net;
using LibStored.Net.Example;
using LibStored.Net.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static LibStored.Net.ProtocolBuilder;

// Example of using the LibStored library with a simple store
ExampleStore store = new();
store.Number = 42;
int number = store.Number;
DebugVariant? n = store.Find("/number");

store.Fraction = 3.14;
double faction = store.Fraction;

Console.WriteLine(number);
Console.WriteLine(faction);

// Example of using the ProtocolLayer with logging
IServiceCollection services = new ServiceCollection();
services.AddLogging(x => x.AddConsole());

// Registering the ExampleStore as a singleton service, so it can be re-used across the application.
// For now directly use the store instance, but in a real application you would typically use dependency injection to get the store instance.
services.AddSingleton<ExampleStore>(store);

// Registering the default LibStored.Net ProtocolLayer, custom layers and its dependencies.
// Add as transient services to ensure a new instance is created each time, so also multiple instances of the same layer type can be used.
services.AddLibStoredProtocolLayers();
services.AddTransient<PrintAsciiEncodeLayer>();
services.AddDebugger((sp, x) => x.Map(sp.GetRequiredService<ExampleStore>()));

services.AddProtocolStack(builder => builder
    .Add<Debugger>()
    .Add<PrintAsciiEncodeLayer>()
    .Add<LoggerLayer>()
);

IServiceProvider serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateOnBuild = true,
    ValidateScopes = true,
});

ProtocolStack layer = serviceProvider.GetRequiredService<ProtocolStack>();
string art = layer.ToAsciiArt();
Console.WriteLine(art);


Send("l");
Send("r/number");
Send("w18/number");
Send("r/number");

return;

void Send(string text) => layer.Decode(Encoding.ASCII.GetBytes(text));

namespace LibStored.Net.Example
{
    internal class PrintAsciiEncodeLayer : ProtocolLayer
    {
        /// <inheritdoc />
        public override void Encode(ReadOnlySpan<byte> buffer, bool last)
        {
            string text = Encoding.ASCII.GetString(buffer);
            Console.WriteLine($"Send: {text}");
            base.Encode(buffer, last);
        }

        /// <inheritdoc />
        public override void Decode(Span<byte> buffer)
        {
            string text = Encoding.ASCII.GetString(buffer);
            Console.WriteLine($"Recv: {text}");
            base.Decode(buffer);
        }
    }
}
