// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Nodes;

namespace MartinCostello.EurovisionHue;

internal sealed class AppOptions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    public string ArticleSelector { get; set; } = string.Empty;

    public TimeSpan DiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public string FeedUrl { get; set; } = string.Empty;

    public string HueToken { get; set; } = string.Empty;

    public IList<Guid> LightIds { get; set; } = [];

    internal string UserSettings { get; set; } = "usersettings.json";

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        var userSettings = new JsonObject()
        {
            [nameof(HueToken)] = HueToken,
            [nameof(LightIds)] = new JsonArray([.. LightIds]),
        };

        var directory = Path.GetDirectoryName(UserSettings);

        if (directory is not null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(userSettings, Options);
        await File.WriteAllTextAsync(UserSettings, json, cancellationToken);
    }
}
