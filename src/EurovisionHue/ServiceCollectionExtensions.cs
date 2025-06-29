// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal static class ServiceCollectionExtensions
{
    private static readonly string Version = typeof(ServiceCollectionExtensions)
        .Assembly
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
        .InformationalVersion;

    public static IServiceCollection AddEurovisionHue(
        this IServiceCollection services,
        IConfigurationRoot configuration)
    {
        services.AddSingleton<App>()
                .AddSingleton<EurovisionFeed>()
                .AddSingleton<GitHubGistConfiguration>()
                .AddSingleton<LightsClientFactory>()
                .AddSingleton(AnsiConsole.Console)
                .AddSingleton(Random.Shared)
                .AddSingleton(TimeProvider.System)
                .AddSingleton(configuration)
                .AddTransient<IPostConfigureOptions<AppOptions>, GitHubGistAppOptions>();

        services.Configure<AppOptions>(configuration);

        services.AddHttpClient()
                .ConfigureHttpClientDefaults((options) =>
                {
                    options.AddStandardResilienceHandler();
                    options.ConfigureHttpClient((client) => client.DefaultRequestHeaders.UserAgent.Add(new("MartinCostello.EurovisionHue", Version)));
                    options.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    });
                });

        return services;
    }
}
