// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.EurovisionHue;

internal sealed class AppConfiguration
{
    public string? ArticleSelector { get; set; }

    public string? FeedUrl { get; set; }

    public string? HueToken { get; set; }

    public IList<Guid> LightIds { get; set; } = [];
}
