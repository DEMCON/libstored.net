# Arq Example

This example demonstrates cross-language synchronization over a lossy channel. The Arq layer will retransmist lost messages between C++ and C# components. The C++ component runs inside a container while the C# implementation runs natively. The setup is adapted from the libstored project's synchronization examples and uses the Arq transport/adapter to route changes between the stores.

In this example, the C++ code runs inside a container, while the C# implementation runs natively.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Aspire CLI](https://aspire.dev/docs/cli/)
- [Podman](https://podman.io/) or [Docker](https://www.docker.com/products/docker-desktop)

## Running the Example

Start the example environment with Aspire:

```sh
aspire run
```

## Learn More

- [Aspire Documentation](https://aspire.dev/docs/)