// SPDX-FileCopyrightText: 2025 Guus Kuiper
// 
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace LibStored.Net.Example;

/// <summary>
/// Compare the HttpClient pipeline setup to the ProtocolLayer in libstored.
/// </summary>
public static class HttpClientExtensions
{
	public static IServiceCollection AddHttpClient(this IServiceCollection services)
    {
        services.AddTransient<LoggingHandler>();
		services.AddHttpClient("StoredClient", client =>
		{
			client.BaseAddress = new Uri("https://api.example.com/");
			client.DefaultRequestHeaders.Add("Accept", "application/json");
		}).AddHttpMessageHandler<LoggingHandler>();

		return services;
	}

	public static HttpClient CreateClient()
	{
		var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
			.AddRetry(new HttpRetryStrategyOptions
			{
				BackoffType = DelayBackoffType.Exponential,
				MaxRetryAttempts = 3
			})
			.Build();

		SocketsHttpHandler socketHandler = new()
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(5),
			KeepAlivePingDelay = TimeSpan.FromSeconds(30),
			KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
			EnableMultipleHttp2Connections = true
		};

		ResilienceHandler resilienceHandler = new (retryPipeline)
		{
			InnerHandler = socketHandler,
		};

		var client = new HttpClient(resilienceHandler)
		{
			BaseAddress = new Uri("https://api.example.com/")
		};

		return client;
	}

	public class LoggingHandler : DelegatingHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Console.WriteLine($"Sending request to {request.RequestUri}");
			var response = await base.SendAsync(request, cancellationToken);
			Console.WriteLine($"Received response with status code {response.StatusCode}");
			return response;
		}
	}
}