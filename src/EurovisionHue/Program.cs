// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

// Based on https://github.com/cnorthwood/eurovisionhue
using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models.Requests;
using MartinCostello.EurovisionHue;
using SixLabors.ImageSharp.PixelFormats;

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

var feed = new EurovisionFeed(
    "https://www.bbc.co.uk/news/live/c74n9n5l1nxt",
    "article[data-testid='content-post']");

await foreach (var participant in feed.GetParticipantsAsync().WithCancellation(cts.Token))
{
    Console.WriteLine($"Country changed to {participant}");

    var colors = participant.Colors(bulbs.Count);

    foreach ((var index, var light) in bulbs.Index())
    {
        var color = colors[index % colors.Count];
        (var x, var y) = RgbToXY(color);

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

static (double X, double Y) RgbToXY(Rgba32 color)
{
    // See https://gist.github.com/popcorn245/30afa0f98eea1c2fd34d
    var r = GammaCorrection(color.R / 255.0);
    var g = GammaCorrection(color.G / 255.0);
    var b = GammaCorrection(color.B / 255.0);

    var x = (r * 0.649926) + (g * 0.103455) + (b * 0.197109);
    var y = (r * 0.234327) + (g * 0.743075) + (b * 0.022598);
    var z = (r * 0.000000) + (g * 0.053077) + (b * 1.035763);

    var total = x + y + z;

    return (x / total, y / total);

    static double GammaCorrection(double color)
        => color > 0.04045 ? Math.Pow((color + 0.055) / 1.055, 2.4) : color / 12.92;
}
