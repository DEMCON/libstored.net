// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using Aspire.Hosting.Python;

namespace LibStored.Net.Example.AppHost;

public static class ResourceBuilderExtensions
{
#pragma warning disable ASPIREHOSTINGPYTHON001
    public static IResourceBuilder<PythonAppResource> WithPortArgFromEndpoint(this IResourceBuilder<PythonAppResource> resourceBuilder, IResourceBuilder<IResourceWithEndpoints> endpointResource, string endpointName)
#pragma warning restore ASPIREHOSTINGPYTHON001
    {
        return resourceBuilder.WithArgs(context =>
        {
            EndpointReference endpoint = endpointResource.GetEndpoint("debug");

            // Use .Property to access Host and Port expressions and defer evaluation
            EndpointReferenceExpression port = endpoint.Property(EndpointProperty.Port);

            context.Args.Add("-p");
            context.Args.Add(port);
        });
    }
}