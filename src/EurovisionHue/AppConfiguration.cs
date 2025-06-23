// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.EurovisionHue;

internal sealed class AppConfiguration
{
    public string ArticleSelector { get; set; } = "article[data-testid='content-post']";

    public string FeedUrl { get; set; } = "https://www.bbc.co.uk/news/live/c74n9n5l1nxt";

    public string? HueToken { get; set; }

    public IList<Guid> LightIds { get; set; } = [];
}
