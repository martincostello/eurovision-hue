// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models.Clip;
using HueApi.Models.Exceptions;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal class LightsClientFactory(
    HttpClient client,
    IAnsiConsole console,
    IOptions<AppOptions> options)
{
    private const string ApplicationName = "EurovisionHue";

    public virtual async Task<LightsClient?> CreateAsync(CancellationToken cancellationToken)
    {
        var fastTimeout = options.Value.DiscoveryTimeout;
        var maxTimeout = fastTimeout * 4;

        var bridges = await console
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(
                "Searching for Hue bridges...",
                async (_) => await DiscoverAsync(fastTimeout, maxTimeout));

        LocatedBridge bridge;

        if (bridges.Count < 1)
        {
            console.WriteErrorLine("No Hue bridges found.");
            return null;
        }
        else if (bridges.Count is 1)
        {
            bridge = bridges[0];
            console.MarkupLineInterpolated($"Found Hue bridge at [link={bridge.IpAddress}]{bridge.Url}[/].");
            console.WriteLine();
        }
        else
        {
            bridge = console.Prompt(
                new SelectionPrompt<LocatedBridge>()
                    .Title("Which Hue bridge do you want to use?")
                    .MoreChoicesText("[grey](Move up and down to see more bridges)[/]")
                    .AddChoices(bridges)
                    .UseConverter((p) => $"[link={p.IpAddress}]{p.Url}[/]"));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        var token = options.Value.HueToken;

        if (string.IsNullOrEmpty(token))
        {
            var confirmed = console.Prompt(
                new ConfirmationPrompt(
                    string.Join(
                        Environment.NewLine,
                        [
                            "The button on the Hue bridge needs to be pressed to connect to your lights.",
                            string.Empty,
                            "Press the button on the bridge [bold]first[/], then press [green]Enter[/] to continue.",
                        ])));

            if (!confirmed)
            {
                return null;
            }

            try
            {
                var registration = await RegisterAsync(bridge.IpAddress);

                token = registration?.Username ?? string.Empty;

                var settings = options.Value;

                settings.HueToken = token;
                await settings.SaveAsync(cancellationToken);
            }
            catch (LinkButtonNotPressedException)
            {
                console.WriteErrorLine("The link button on the Hue bridge was not pressed. Unable to connect.");
                return null;
            }
        }

        return new(
            bridge.IpAddress,
            token,
            client,
            console);
    }

    protected virtual async Task<IList<LocatedBridge>> DiscoverAsync(TimeSpan fastTimeout, TimeSpan maxTimeout)
        => await HueBridgeDiscovery.CompleteDiscoveryAsync(fastTimeout, maxTimeout);

    protected virtual async Task<RegisterEntertainmentResult?> RegisterAsync(string ip)
        => await LocalHueApi.RegisterAsync(ip, ApplicationName, Environment.MachineName);
}
