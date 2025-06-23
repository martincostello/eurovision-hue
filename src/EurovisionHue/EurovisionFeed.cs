// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal sealed class EurovisionFeed(
    IOptions<AppOptions> configuration,
    IAnsiConsole console)
{
    private readonly TimeSpan Frequency = TimeSpan.FromSeconds(10);

    public async IAsyncEnumerable<Participant> ParticipantsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();

        var browserType = playwright[BrowserType.Chromium];

        await using var browser = await browserType.LaunchAsync();
        await using var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();

        await console
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(
                "Loading Eurovision feed...",
                async (_) =>
                {
                    await page.GotoAsync(
                        configuration.Value.FeedUrl,
                        new() { WaitUntil = WaitUntilState.NetworkIdle });
                });

        Participant? previous = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            Participant? current = null;

            foreach (var article in await GetArticlesAsync(page))
            {
                if (Participants.TryFind(article, out var participant))
                {
                    current = participant;
                    break;
                }
            }

            if (current is not null && current != previous)
            {
                yield return current;
                previous = current;
            }

            try
            {
                await Task.Delay(Frequency, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task<IReadOnlyList<string>> GetArticlesAsync(IPage page)
    {
        try
        {
            var locator = page.Locator(configuration.Value.ArticleSelector);
            return await locator.AllInnerTextsAsync();
        }
        catch (Exception ex)
        {
            console.WriteException(ex);
            return [];
        }
    }
}
