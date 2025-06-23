// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

// Based on https://github.com/cnorthwood/eurovisionhue
using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models.Requests;
using MartinCostello.EurovisionHue;

// TODO
// 1. Configurable feed URL
// 2. Configurable CSS selector
// 3. Configurable light names
// 4. Save the Hue token
// 5. Save the light IDs
// 6. CLI interface

var timeout = TimeSpan.FromSeconds(30);

var locator = new LocalNetworkScanBridgeLocator();
var bridges = await HueBridgeDiscovery.CompleteDiscoveryAsync(timeout, timeout);
var bridge = bridges.First();

Console.WriteLine("Press the button on the Hue bridge...");
Console.ReadLine();

var registration = await LocalHueApi.RegisterAsync(
    bridge.IpAddress,
    "EurovisionHue",
    Environment.MachineName);

if (registration is null)
{
    // TODO
    return;
}

var client = new LocalHueApi(bridge.IpAddress, registration.Username);
var lights = await client.GetLightsAsync();

var bulbs = lights.Data
    .Where((p) => p.Metadata!.Name.Contains("living", StringComparison.OrdinalIgnoreCase))
    .ToList();

foreach ((var index, var light) in bulbs.Index())
{
    Console.WriteLine($"{index + 1}: {light.Metadata?.Name}");
}

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;

    if (!cts.IsCancellationRequested)
    {
        cts.Cancel();
    }
};

var configuration = new AppConfiguration();

var feed = new EurovisionFeed(
    configuration.FeedUrl,
    configuration.ArticleSelector);

await foreach (var participant in feed.GetParticipantsAsync().WithCancellation(cts.Token))
{
    Console.Out.WriteLine($"[{DateTime.Now:t}] Changed to {participant.Emoji} {participant.Name}");

    var colors = participant.Colors(bulbs.Count);

    foreach ((var index, var light) in bulbs.Index())
    {
        var color = colors[index % colors.Count];
        (var x, var y) = color.ToXY();

        try
        {
            var request = new UpdateLight()
              .SetColor(x, y)
              .SetDuration(TimeSpan.FromSeconds(5))
              .TurnOn();

            await client.UpdateLightAsync(light.Id, request);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to update light {light.Metadata?.Name} ({light.Id}): {ex.Message}");
        }
    }
}
