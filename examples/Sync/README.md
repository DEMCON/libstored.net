# Sync Example

This example demonstrates cross-language synchronization between C++ and C# using the [Aspire CLI](https://aspire.dev/docs/cli/). The setup is based on the [8_sync](https://github.com/DEMCON/libstored/tree/main/examples/8_sync) example from the libstored project.

In this example, the C++ code runs inside a container, while the C# implementation runs natively. The `libstored.gui` Python library connects to both the C++ and C# components, allowing you to observe and interact with the synchronizated stores.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Aspire CLI](https://aspire.dev/docs/cli/)
- [Podman](https://podman.io/) or [Docker](https://www.docker.com/products/docker-desktop)

## Running the Example

```sh
aspire run
```

## Learn More

- [Aspire Documentation](https://aspire.dev/docs/)