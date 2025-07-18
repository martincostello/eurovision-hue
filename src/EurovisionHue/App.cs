﻿// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using HueApi.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Spectre.Console;
using ConsoleColor = Spectre.Console.Color;

namespace MartinCostello.EurovisionHue;

internal sealed class App(
    LightsClientFactory clientFactory,
    IConfigurationRoot configuration,
    IAnsiConsole console,
    EurovisionFeed feed,
    Random random,
    IOptions<AppOptions> options) : IAsyncDisposable, IDisposable
{
    private Timer? _reloadTimer;

    public static async Task<int> RunAsync(
        Action<IServiceCollection> configure,
        CancellationToken cancellationToken)
    {
        var userSettings = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MartinCostello",
            "EurovisionHue",
            "usersettings.json");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(userSettings, optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<App>()
            .Build();

        var services = new ServiceCollection();

        services.AddEurovisionHue(configuration);

        services.AddOptions<AppOptions>()
                .PostConfigure((p) => p.UserSettings = userSettings);

        configure(services);

        using var provider = services.BuildServiceProvider();

        await using var app = provider.GetRequiredService<App>();

        return await app.RunAsync(cancellationToken) ? 0 : 1;
    }

    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        console.Write(new FigletText("Eurovision Hue").Color(ConsoleColor.Gold1));
        console.WriteLine();

        var client = await clientFactory.CreateAsync(cancellationToken);

        if (client is null)
        {
            return false;
        }

        var settings = options.Value;
        var lights = await client.GetLightsAsync();

        if (settings.LightIds.Count > 0)
        {
            lights = [.. lights.Where((p) => settings.LightIds.Contains(p.Id))];
        }
        else
        {
            lights = console.Prompt(
                new MultiSelectionPrompt<Light>()
                    .Title("Which lights do you want to follow the contest with?")
                    .InstructionsText("[grey](Press [blue]<space>[/] to toggle a light, [green]<enter>[/] to accept)[/]")
                    .MoreChoicesText("[grey](Move up and down to see more lights)[/]")
                    .Required()
                    .AddChoices(lights)
                    .UseConverter((p) => p.Metadata?.Name ?? p.Id.ToString()));

            settings.LightIds = [.. lights.Select((p) => p.Id)];
            await settings.SaveAsync(cancellationToken);
        }

        console.WriteLine();

        if (!string.IsNullOrEmpty(settings.ConfigurationGistId) &&
            settings.ReloadFrequency is { Ticks: > 0 } frequency)
        {
            _reloadTimer = new Timer(
                static (state) => ((IConfigurationRoot)state!).Reload(),
                configuration,
                frequency,
                frequency);
        }

        try
        {
            await foreach (var participant in feed.ParticipantsAsync(cancellationToken).WithCancellation(cancellationToken))
            {
                await console
                    .Status()
                    .Spinner(Spinner.Known.Dots)
                    .StartAsync(
                        "Updating lights...",
                        async (_) =>
                        {
                            var colors = participant.Colors(lights.Count);

                            Light[] shuffled = [.. lights];

                            random.Shuffle(shuffled);

                            foreach ((var index, var light) in shuffled.Index())
                            {
                                var color = colors[index % colors.Count];
                                await client.ChangeAsync(light, color);
                            }
                        });
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or PlaywrightException)
        {
            // Ignore
        }

        return true;
    }

    public void Dispose() => _reloadTimer?.Dispose();

    public async ValueTask DisposeAsync()
    {
        if (_reloadTimer is { } timer)
        {
            await timer.DisposeAsync();
        }
    }
}
