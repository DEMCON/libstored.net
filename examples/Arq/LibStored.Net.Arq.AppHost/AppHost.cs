using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

const string libstoredVersion = "2.0.0";
var arq = builder.AddDockerfile("libstored-arq", "../LibStored.Arq/", "Dockerfile")
        .WithImage("demcon/libstored-arq-example")
        .WithImageTag(libstoredVersion)
        .WithBuildArg("LIBSTORED_VERSION", libstoredVersion)
        .WithEndpoint(targetPort: 5555, name: "sync", protocol: ProtocolType.Tcp)
        .WithLifetime(ContainerLifetime.Session)
    ;

builder.AddProject<Projects.LibStored_Net_Arq_Console>("csharp")
    .WaitFor(arq)
    .WithEnvironment(context =>
    {
        var endpoint = arq.GetEndpoint("sync");

        // Use .Property to access Host and Port expressions and defer evaluation
        var hostAndPort = endpoint.Property(EndpointProperty.HostAndPort);

        // Use ReferenceExpression for deferred resolution
        context.EnvironmentVariables["NETMQ_CLIENT"] = ReferenceExpression.Create($">tcp://{hostAndPort}");
    })
    ;

builder.Build().Run();
