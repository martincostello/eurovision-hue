// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using HueApi;
using HueApi.Models;
using HueApi.Models.Requests;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal sealed class LightsClient(
    string ip,
    string key,
    HttpClient client,
    IAnsiConsole console)
{
    private static readonly TimeSpan Duration = TimeSpan.FromSeconds(3);
    private readonly LocalHueApi _client = new(ip, key, client);

    public async Task<IReadOnlyList<Light>> GetLightsAsync()
    {
        var response = await _client.Light.GetAllAsync();
        return [.. response.Data.OrderBy((p) => p.Metadata?.Name)];
    }

    public async Task ChangeAsync(Light light, Color color)
    {
        (var x, var y) = color.ToXY();

        try
        {
            var request = new UpdateLight()
              .SetColor(x, y)
              .SetDuration(Duration)
              .TurnOn();

            await _client.Light.UpdateAsync(light.Id, request);
        }
        catch (Exception ex)
        {
            console.WriteException(ex);
        }
    }
}
