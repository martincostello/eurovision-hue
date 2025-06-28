// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

internal sealed class EurovisionFeed : IDisposable
{
    private readonly IDisposable? _changed;
    private readonly IAnsiConsole _console;
    private readonly TimeSpan _frequency;
    private readonly TimeProvider _timeProvider;

    private CancellationTokenSource _reloaded;
    private string _feedUrl;
    private string _selector;

    public EurovisionFeed(
        IAnsiConsole console,
        TimeProvider timeProvider,
        IOptionsMonitor<AppOptions> options)
    {
        _console = console;
        _timeProvider = timeProvider;

        var snapshot = options.CurrentValue;

        _feedUrl = snapshot.FeedUrl!;
        _selector = snapshot.ArticleSelector;
        _frequency = snapshot.FeedFrequency;

        _reloaded = new CancellationTokenSource();
        _changed = options.OnChange((updated, _) =>
        {
            if (updated.FeedUrl != _feedUrl || updated.ArticleSelector != _selector)
            {
                _feedUrl = updated.FeedUrl;
                _selector = updated.ArticleSelector;

                var previous = Interlocked.Exchange(ref _reloaded, new CancellationTokenSource());
                previous.Cancel();
                previous.Dispose();
            }
        });
    }

    public void Dispose()
    {
        _changed?.Dispose();
        _reloaded.Dispose();
    }

    public async IAsyncEnumerable<Participant> ParticipantsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var reloaded = _reloaded.Token;
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, reloaded);

            await foreach (var participant in ParticipantsAsync(_feedUrl, _selector, _frequency, linked.Token))
            {
                yield return participant;
            }
        }
    }

    private async IAsyncEnumerable<Participant> ParticipantsAsync(
        string feedUrl,
        string selector,
        TimeSpan frequency,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var playwright = await Playwright.CreateAsync();

        var browserType = playwright[BrowserType.Chromium];

        await using var browser = await browserType.LaunchAsync();
        await using var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();

        await _console
            .Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(
                $"Loading Eurovision feed from {_feedUrl}...",
                async (_) =>
                {
                    await page.GotoAsync(
                        feedUrl,
                        new() { WaitUntil = WaitUntilState.NetworkIdle });
                });

        while (!cancellationToken.IsCancellationRequested)
        {
            Participant? current = null;

            foreach (var article in await GetArticlesAsync(page, selector))
            {
                if (Participants.TryFind(article, out var participant))
                {
                    current = participant;
                    break;
                }
            }

            current ??= Participant.Unknown;

            if (current != null)
            {
                var now = _timeProvider.GetLocalNow();
                _console.WriteLine($"[{now:t}] Found {current.Emoji} {current.Name}");

                yield return current;
            }

            try
            {
                await Task.Delay(frequency, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task<IReadOnlyList<string>> GetArticlesAsync(IPage page, string selector)
    {
        try
        {
            var locator = page.Locator(selector);
            return await locator.AllInnerTextsAsync();
        }
        catch (Exception ex)
        {
            _console.WriteException(ex);
            return [];
        }
    }
}
