// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

// ReSharper disable SuggestVarOrType_SimpleTypes
// ReSharper disable SuggestVarOrType_Elsewhere

using System.Net.Sockets;
using LibStored.Net.Example.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("docker-compose");

const string libstoredVersion = "1.8.0";
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

#pragma warning disable ASPIREHOSTINGPYTHON001
var py1 = builder.AddPythonApp("libstored-gui-container", "python", "-m", ["libstored.gui"])
    .WaitFor(sync)
    .WithPortArgFromEndpoint(sync, "debug")
    .ExcludeFromManifest()
    ;

var py2 = builder.AddPythonApp("libstored-gui-cs", "python", "-m", ["libstored.gui"])
    .WaitFor(client)
    .WithPortArgFromEndpoint(client, "debug")
    .ExcludeFromManifest()
    ;

#pragma warning restore ASPIREHOSTINGPYTHON001

builder.Build().Run();
