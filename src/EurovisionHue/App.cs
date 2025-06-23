// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using HueApi.Models;
using Microsoft.Extensions.Options;
using Spectre.Console;
using ConsoleColor = Spectre.Console.Color;

namespace MartinCostello.EurovisionHue;

internal sealed class App(
    IOptions<AppOptions> options,
    IAnsiConsole console,
    HttpClient httpClient,
    TimeProvider timeProvider)
{
    public async Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        console.Write(new FigletText("Eurovision Hue").Color(ConsoleColor.Gold1));
        console.WriteLine();

        var factory = new LightsClientFactory(options, console, httpClient);
        var client = await factory.CreateAsync(cancellationToken);

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

        var feed = new EurovisionFeed(
            options,
            console);

        await foreach (var participant in feed.ParticipantsAsync(cancellationToken).WithCancellation(cancellationToken))
        {
            var now = timeProvider.GetLocalNow();

            console.WriteLine($"[{now:t}] Detected {participant.Emoji} {participant.Name}");

            await console
                .Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(
                    "Updating lights...",
                    async (_) =>
                    {
                        var colors = participant.Colors(lights.Count);

                        foreach ((var index, var light) in lights.Index())
                        {
                            var color = colors[index % colors.Count];
                            await client.ChangeAsync(light, color);
                        }
                    });
        }

        return true;
    }
}
