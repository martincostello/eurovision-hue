// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEurovisionHue(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<App>()
                .AddSingleton<EurovisionFeed>()
                .AddSingleton<LightsClientFactory>()
                .AddSingleton(AnsiConsole.Console)
                .AddSingleton(TimeProvider.System);

        services.Configure<AppOptions>(configuration);

        services.AddHttpClient()
                .ConfigureHttpClientDefaults((options) =>
                {
                    options.AddStandardResilienceHandler();
                    options.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    });
                });

        return services;
    }
}
