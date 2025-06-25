// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;

namespace MartinCostello.EurovisionHue;

[CollectionDefinition("Browser")]
public sealed class BrowserFixture : IAsyncLifetime, ICollectionFixture<BrowserFixture>
{
    private static readonly string SolutionRoot = typeof(BrowserFixture).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .First((p) => p.Key is "SolutionRoot")
        .Value!;

    private static readonly string DemoHtmlFile = GetPageUrl(SolutionRoot, ".", "demo.html");

    private static readonly string InvalidHtmlFile = GetPageUrl(SolutionRoot, "tests", "EurovisionHue.Tests", "invalid.html");

    public string ArticleSelector { get; } = "td:nth-child(2)";

    public string FeedUrl { get; } = DemoHtmlFile;

    public string InvalidArticleSelector { get; } = "td:nth-child(1)";

    public string InvalidFeedUrl { get; } = InvalidHtmlFile;

    public ValueTask InitializeAsync()
    {
        InstallPlaywright();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    private static string GetPageUrl(params string[] paths)
    {
        var feedUrl = Path.Combine(paths);
        return new Uri($"file://{feedUrl}").ToString();
    }

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(["install", "chromium", "--only-shell"]);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }
}
