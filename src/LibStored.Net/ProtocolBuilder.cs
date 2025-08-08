// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using static LibStored.Net.ProtocolBuilder;

namespace LibStored.Net;

public static class ProtocolBuilderExtensions
{
    public static IServiceCollection AddDebugger(this IServiceCollection services, Action<Debugger> builder)
    {
        services.AddTransient<Debugger>(sp =>
        {
            Debugger debugger = new();
            builder(debugger);
            return debugger;
        });
        return services;
    }

    public static IServiceCollection AddDebugger(this IServiceCollection services, Action<IServiceProvider, Debugger> builder)
    {
        services.AddTransient<Debugger>(sp =>
        {
            Debugger debugger = new();
            builder(sp, debugger);
            return debugger;
        });
        return services;
    }

    public static IServiceCollection AddProtocolStack(this IServiceCollection services, Action<ProtocolBuilder> builderActions)
    {
        services.AddSingleton<ProtocolStack>(sp =>
        {
            ProtocolBuilder builder = ProtocolBuilder.Create(sp);
            builderActions(builder);
            return builder.Build();
        });
        return services;
    }
}

public class ProtocolBuilder
{
    private readonly IServiceProvider _services;
    private readonly IList<Protocol.ProtocolLayer> _layers = [];

    public ProtocolBuilder(IServiceProvider services)
    {
        _services = services;
    }

    public static ProtocolBuilder Create(IServiceProvider services)
    {
        return new ProtocolBuilder(services);
    }

    public ProtocolBuilder Add(Protocol.ProtocolLayer layer)
    {
        _layers.Add(layer);

        return this;
    }

    public ProtocolBuilder Add<T>(T layer) where T : Protocol.ProtocolLayer
    {
        _layers.Add(layer);

        return this;
    }

    public ProtocolBuilder Add<T>() where T : Protocol.ProtocolLayer
    {
        Protocol.ProtocolLayer layer = _services.GetRequiredService<T>();

        return Add(layer);
    }

    public ProtocolBuilder Add<T>(Action<T> configure) where T : Protocol.ProtocolLayer
    {
        T layer = _services.GetRequiredService<T>();

        configure(layer);

        return Add(layer);
    }

    public ProtocolStack Build(bool reserver = false)
    {
        List<Protocol.ProtocolLayer> stack = [];

        if (reserver)
        {
            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                Protocol.ProtocolLayer layer = _layers[i];
                if (stack.Count > 0)
                {
                    layer.Wrap(stack[^1]);
                }

                stack.Add(layer);
            }
        }
        else
        {
            for (int i = 0; i < _layers.Count; i++)
            {
                Protocol.ProtocolLayer layer = _layers[i];
                if (stack.Count > 0)
                {
                    layer.Wrap(stack[^1]);
                }

                stack.Add(layer);
            }
        }

        return new ProtocolStack(stack);
    }

    public class ProtocolStack : Protocol.ProtocolLayer
    {
        private readonly List<Protocol.ProtocolLayer> _layers;

        public static ProtocolStack Traverse(Protocol.ProtocolLayer layer)
        {
            Protocol.ProtocolLayer current = layer;

            // Find the top layer.
            while (current.Up() is not null)
            {
                current = current.Up()!;
            }

            List<Protocol.ProtocolLayer> layers = [current];

            // Traverse down to the bottom layer.
            while (current.Down() is not null)
            {
                current = current.Down()!;
                layers.Add(current);
            }

            return new ProtocolStack(layers);
        }

        public ProtocolStack(List<Protocol.ProtocolLayer> layers)
        {
            if (layers.Count == 0)
            {
                layers = [new Protocol.ProtocolLayer()];
            }

            _layers = layers;
        }

        public IEnumerable<Protocol.ProtocolLayer> Layers => _layers;

        public override void Decode(Span<byte> buffer) => _layers[^1].Decode(buffer);
        public override void Encode(ReadOnlySpan<byte> buffer, bool last) => _layers[0].Encode(buffer, last);

        public string ToAsciiArt()
        {
            StringBuilder sb = new();

            sb.AppendLine();

            int count = _layers.Count;
            for (int i = 0; i < count; i++)
            {
                string typeName = _layers[i].GetType().Name;
                string label = $"Layer {i + 1}: {typeName}";
                int boxWidth = Math.Max(label.Length, 35);
                string border = "+" + new string('-', boxWidth) + "+";
                string content = "| " + label.PadRight(boxWidth - 2) + " |";

                sb.AppendLine(border);
                sb.AppendLine(content);
                sb.AppendLine(border);

                if (i < count - 1)
                {
                    sb.AppendLine("".PadLeft(boxWidth / 2 + 1) + "|");
                }
            }

            sb.AppendLine();

            return sb.ToString();
        }
    }
}