// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Nodes;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal sealed class GitHubGistConfiguration(HttpClient client, IAnsiConsole console)
{
    public (string? FeedUrl, string? ArticleSelector) Get(string id, CancellationToken cancellationToken = default)
    {
        string? feedUrl = null;
        string? articleSelector = null;

        if (GetGist(id, cancellationToken) is JsonObject gist &&
            gist.TryGetPropertyValue("files", out var files) &&
            files is JsonObject files2)
        {
            foreach ((_, var file) in files2)
            {
                if (file is not JsonObject fileObject ||
                    !fileObject.TryGetPropertyValue("type", out var type) ||
                    type is null ||
                    type.GetValueKind() != JsonValueKind.String ||
                    type.GetValue<string>() is not "application/json")
                {
                    continue;
                }

                if (!fileObject.TryGetPropertyValue("content", out var content) ||
                    content is not JsonValue contentValue ||
                    contentValue.GetValueKind() != JsonValueKind.String)
                {
                    continue;
                }

                var json = contentValue.GetValue<string>();

                if (JsonObject.Parse(json) is JsonObject config)
                {
                    if (config.TryGetPropertyValue(nameof(AppOptions.FeedUrl), out var feedUrlValue) &&
                        feedUrlValue is JsonValue feedUrlJson &&
                        feedUrlJson.GetValueKind() == JsonValueKind.String)
                    {
                        feedUrl = feedUrlJson.GetValue<string>();
                    }

                    if (config.TryGetPropertyValue(nameof(AppOptions.ArticleSelector), out var articleSelectorValue) &&
                        articleSelectorValue is JsonValue articleSelectorJson &&
                        articleSelectorJson.GetValueKind() == JsonValueKind.String)
                    {
                        articleSelector = articleSelectorJson.GetValue<string>();
                    }
                }
            }
        }

        return (feedUrl, articleSelector);
    }

    private JsonObject? GetGist(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/gists/{id}");
            using var response = client.Send(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                console.WriteErrorLine($"Failed to get GitHub gist {id}: {response.StatusCode} {response.ReasonPhrase}");
                return null;
            }

            using var stream = response.Content.ReadAsStream(cancellationToken);

            var json = JsonNode.Parse(stream);

            if (json is not JsonObject gist)
            {
                return null;
            }

            return gist;
        }
        catch (Exception ex)
        {
            console.WriteException(ex);
            return null;
        }
    }
}
