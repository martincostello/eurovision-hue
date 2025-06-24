// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace MartinCostello.EurovisionHue;

internal sealed class GitHubGistAppOptions(GitHubGistConfiguration configuration) : IPostConfigureOptions<AppOptions>
{
    public void PostConfigure(string? name, AppOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConfigurationGistId))
        {
            (var feedUrl, var articleSelector) = configuration.Get(options.ConfigurationGistId);

            if (!string.IsNullOrWhiteSpace(feedUrl))
            {
                options.FeedUrl = feedUrl;
            }

            if (!string.IsNullOrWhiteSpace(articleSelector))
            {
                options.ArticleSelector = articleSelector;
            }
        }
    }
}
