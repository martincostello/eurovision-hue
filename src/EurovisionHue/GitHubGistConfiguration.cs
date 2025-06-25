// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal sealed partial class GitHubGistConfiguration(HttpClient client, IAnsiConsole console)
{
    public (string? FeedUrl, string? ArticleSelector) Get(string id, CancellationToken cancellationToken = default)
    {
        string? feedUrl = null;
        string? articleSelector = null;

        if (GetGist(id, cancellationToken) is { Files.Count: > 0 } gist)
        {
            foreach ((_, var file) in gist.Files)
            {
                if (file.Type is not "application/json" || string.IsNullOrWhiteSpace(file.Content))
                {
                    continue;
                }

                try
                {
                    var feed = JsonSerializer.Deserialize<FeedConfiguration>(
                        file.Content,
                        GitHubJsonSerializerContext.Default.FeedConfiguration);

                    feedUrl ??= feed?.FeedUrl;
                    articleSelector ??= feed?.ArticleSelector;
                }
                catch (JsonException ex)
                {
                    console.WriteException(ex);
                }
            }
        }

        return (feedUrl, articleSelector);
    }

    private Gist? GetGist(string id, CancellationToken cancellationToken = default)
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

            return JsonSerializer.Deserialize<Gist>(stream, GitHubJsonSerializerContext.Default.Gist);
        }
        catch (Exception ex)
        {
            console.WriteException(ex);
            return null;
        }
    }

    private sealed class FeedConfiguration
    {
        [JsonPropertyName(nameof(AppOptions.ArticleSelector))]
        public string? ArticleSelector { get; set; }

        [JsonPropertyName(nameof(AppOptions.FeedUrl))]
        public string? FeedUrl { get; set; }
    }

    private sealed class Gist
    {
        [JsonPropertyName("files")]
        public IDictionary<string, GistFile>? Files { get; set; }
    }

    private sealed class GistFile
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [JsonSerializable(typeof(FeedConfiguration))]
    [JsonSerializable(typeof(Gist))]
    private sealed partial class GitHubJsonSerializerContext : JsonSerializerContext;
}
