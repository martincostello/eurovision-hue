// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace MartinCostello.EurovisionHue;

internal sealed class EurovisionFeed(string url, string selector)
{
    private readonly TimeSpan Frequency = TimeSpan.FromSeconds(10);

    public async IAsyncEnumerable<Participant> GetParticipantsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();

        var browserType = playwright[BrowserType.Chromium];

        await using var browser = await browserType.LaunchAsync();
        await using var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();

        await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });

        Participant? previous = null;

        while (!cancellationToken.IsCancellationRequested)
        {
            Participant? current = null;

            foreach (var article in await GetArticlesAsync(page))
            {
                var text = await article.InnerTextAsync();

                if (Participants.TryFind(text, out var participant))
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

    private async Task<IReadOnlyList<IElementHandle>> GetArticlesAsync(IPage page)
    {
        try
        {
            return await page.QuerySelectorAllAsync(selector);
        }
        catch (Exception)
        {
            // TODO Catch more specific 
            return [];
        }
    }
}
