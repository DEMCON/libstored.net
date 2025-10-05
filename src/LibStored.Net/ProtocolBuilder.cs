// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using System.Text;
using LibStored.Net.Synchronization;
using Microsoft.Extensions.DependencyInjection;

namespace LibStored.Net;

/// <summary>
/// Extension methods for registering protocol layers and debuggers in the dependency injection container.
/// </summary>
public static class ProtocolBuilderExtensions
{
    /// <summary>
    /// Registers a <see cref="Debugger"/> with custom configuration in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the debugger to.</param>
    /// <param name="builder">The action to configure the debugger instance.</param>
    /// <returns>The updated service collection.</returns>
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

    /// <summary>
    /// Registers a <see cref="Debugger"/> with custom configuration and access to the service provider.
    /// </summary>
    /// <param name="services">The service collection to add the debugger to.</param>
    /// <param name="builder">The action to configure the debugger instance with the service provider. Use this for example to map stores to this  <see cref="Debugger"/>.</param>
    /// <returns>The updated service collection.</returns>
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

    /// <summary>
    /// Registers a protocol stack with custom configuration in the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the protocol stack to.</param>
    /// <param name="builderActions">The action to configure the protocol builder.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddProtocolStack(this IServiceCollection services, Action<ProtocolBuilder> builderActions)
    {
        services.AddSingleton<ProtocolBuilder.ProtocolStack>(sp =>
        {
            ProtocolBuilder builder = ProtocolBuilder.Create(sp);
            builderActions(builder);
            return builder.Build();
        });
        return services;
    }
}

/// <summary>
/// Builds and manages a stack of protocol layers for message processing.
/// </summary>
public class ProtocolBuilder
{
    private readonly IServiceProvider _services;
    private readonly IList<Protocol.ProtocolLayer> _layers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ProtocolBuilder"/> class.
    /// </summary>
    /// <param name="services">The service provider for resolving protocol layers.</param>
    private ProtocolBuilder(IServiceProvider services)
    {
        _services = services;
    }

    /// <summary>
    /// Creates a new <see cref="ProtocolBuilder"/> using the specified service provider.
    /// </summary>
    /// <param name="services">The service provider for resolving protocol layers.</param>
    /// <returns>A new <see cref="ProtocolBuilder"/> instance.</returns>
    public static ProtocolBuilder Create(IServiceProvider services)
    {
        return new ProtocolBuilder(services);
    }

    /// <summary>
    /// Adds a protocol layer to the builder.
    /// </summary>
    /// <param name="layer">The protocol layer to add.</param>
    /// <returns>The current <see cref="ProtocolBuilder"/> instance.</returns>
    public ProtocolBuilder Add(Protocol.ProtocolLayer layer)
    {
        _layers.Add(layer);
        return this;
    }

    /// <summary>
    /// Adds a protocol layer of type <typeparamref name="T"/> to the builder.
    /// </summary>
    /// <typeparam name="T">The type of protocol layer to add.</typeparam>
    /// <param name="layer">The protocol layer instance to add.</param>
    /// <returns>The current <see cref="ProtocolBuilder"/> instance.</returns>
    public ProtocolBuilder Add<T>(T layer) where T : Protocol.ProtocolLayer
    {
        _layers.Add(layer);
        return this;
    }

    /// <summary>
    /// Creates and adds a <see cref="SyncConnection"/> from a <see cref="Synchronizer"/>.
    /// </summary>
    /// <param name="synchronizer">The synchronizer used to create the connection layer.</param>
    /// <returns>The current <see cref="ProtocolBuilder"/> instance.</returns>
    public ProtocolBuilder Add(Synchronizer synchronizer)
    {
        _layers.Add(synchronizer.CreateConnectionLayer());
        return this;
    }

    /// <summary>
    /// Adds a protocol layer of type <typeparamref name="T"/> resolved from the service provider.
    /// </summary>
    /// <typeparam name="T">The type of protocol layer to add.</typeparam>
    /// <returns>The current <see cref="ProtocolBuilder"/> instance.</returns>
    public ProtocolBuilder Add<T>() where T : Protocol.ProtocolLayer
    {
        Protocol.ProtocolLayer layer = _services.GetRequiredService<T>();
        return Add(layer);
    }

    /// <summary>
    /// Adds a protocol layer of type <typeparamref name="T"/> resolved from the service provider and configures it.
    /// </summary>
    /// <typeparam name="T">The type of protocol layer to add.</typeparam>
    /// <param name="configure">The action to configure the protocol layer.</param>
    /// <returns>The current <see cref="ProtocolBuilder"/> instance.</returns>
    public ProtocolBuilder Add<T>(Action<T> configure) where T : Protocol.ProtocolLayer
    {
        T layer = _services.GetRequiredService<T>();
        configure(layer);
        return Add(layer);
    }

    /// <summary>
    /// Builds the protocol stack from the added layers.
    /// </summary>
    /// <param name="reserver">If true, reverses the order of the layers.</param>
    /// <returns>A <see cref="ProtocolStack"/> representing the built stack.</returns>
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

    /// <summary>
    /// Represents a stack of protocol layers for message processing.
    /// </summary>
    public class ProtocolStack : Protocol.ProtocolLayer
    {
        private readonly List<Protocol.ProtocolLayer> _layers;

        /// <summary>
        /// Traverses the protocol layer chain and creates a <see cref="ProtocolStack"/> from the top layer.
        /// </summary>
        /// <param name="layer">The starting protocol layer.</param>
        /// <returns>A <see cref="ProtocolStack"/> containing all layers from top to bottom.</returns>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolStack"/> class.
        /// </summary>
        /// <param name="layers">The list of protocol layers in the stack.</param>
        internal ProtocolStack(List<Protocol.ProtocolLayer> layers)
        {
            if (layers.Count == 0)
            {
                layers = [new Protocol.ProtocolLayer()];
            }
            _layers = layers;
        }

        /// <summary>
        /// Gets the protocol layers in the stack.
        /// </summary>
        public IEnumerable<Protocol.ProtocolLayer> Layers => _layers;

        /// <inheritdoc />
        public override void Decode(Span<byte> buffer) => _layers[^1].Decode(buffer);
        /// <inheritdoc />
        public override void Encode(ReadOnlySpan<byte> buffer, bool last) => _layers[0].Encode(buffer, last);

        /// <summary>
        /// Returns an ASCII art representation of the protocol stack.
        /// </summary>
        /// <returns>A string containing the ASCII art representation.</returns>
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
