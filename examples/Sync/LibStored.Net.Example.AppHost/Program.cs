// SPDX-FileCopyrightText: 2025 Guus Kuiper
//
// SPDX-License-Identifier: MIT

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuggestVarOrType_Elsewhere

using System.Net.Sockets;
using LibStored.Net.Example.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("docker-compose");

const string libstoredVersion = "2.1.0";
var sync = builder.AddDockerfile("libstored", ".", "Dockerfile")
    .WithImage("demcon/libstored")
    .WithImageTag(libstoredVersion)
    .WithBuildArg("LIBSTORED_VERSION", libstoredVersion)
    .WithEndpoint(targetPort: 5555, name: "sync", protocol: ProtocolType.Tcp)
    .WithEndpoint(targetPort: 5556, name: "debug", protocol: ProtocolType.Tcp)
    .WithLifetime(ContainerLifetime.Persistent)
    ;

var client = builder.AddProject<Projects.LibStored_Net_Example_Console>("client")
    .WaitFor(sync)
    .WithEnvironment(context =>
    {
        var endpoint = sync.GetEndpoint("sync");

        // Use .Property to access Host and Port expressions and defer evaluation
        var hostAndPort = endpoint.Property(EndpointProperty.HostAndPort);

        // Use ReferenceExpression for deferred resolution
        context.EnvironmentVariables["NETMQ_CLIENT"] = ReferenceExpression.Create($">tcp://{hostAndPort}");
    })
    .WithEndpoint(targetPort: 6666, name: "debug", env: "NETMQ_DEBUG")
    ;

var py1 = builder.AddPythonModule("libstored-gui-container", "python", "libstored.gui")
    .WaitFor(sync)
    .WithPip()
    .WithPortArgFromEndpoint(sync, "debug")
    .ExcludeFromManifest()
    ;

var py2 = builder.AddPythonModule("libstored-gui-cs", "python", "libstored.gui")
    .WaitFor(client)
    .WaitFor(py1) // don't start and install at the same time
    .WithPip(false)
    .WithPortArgFromEndpoint(client, "debug")
    .ExcludeFromManifest()
    ;

builder.Build().Run();
