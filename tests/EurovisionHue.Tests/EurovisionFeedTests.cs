// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;

namespace MartinCostello.EurovisionHue;

[Collection<BrowserFixture>]
public sealed class EurovisionFeedTests(
    BrowserFixture browser,
    ITestOutputHelper outputHelper) : IntegrationTests(browser, outputHelper)
{
    [Fact]
    public async Task Can_Find_Participants()
    {
        // Arrange
        var options = new AppOptions()
        {
            ArticleSelector = Browser.ArticleSelector,
            FeedFrequency = TimeSpan.FromSeconds(5),
            FeedUrl = Browser.FeedUrl,
        };

        var target = new EurovisionFeed(
            Application.Console,
            Application.TimeProvider,
            new AppOptionsSnapshot(options));

        var actual = new List<Participant>();

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, Application.CancellationToken);

        // Act
        await foreach (var participant in target.ParticipantsAsync(combined.Token).WithCancellation(combined.Token))
        {
            actual.Add(participant);
        }

        // Assert
        actual.ShouldNotBeEmpty();
        actual.Distinct().Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    private sealed class AppOptionsSnapshot(AppOptions options) : IOptionsMonitor<AppOptions>
    {
        private Action<AppOptions, string?>? _onChange;

        public AppOptions CurrentValue { get; private set; } = options;

        public void Change(AppOptions options)
        {
            CurrentValue = options;
            _onChange?.Invoke(CurrentValue, null);
        }

        public AppOptions Get(string? name) => CurrentValue;

        public IDisposable? OnChange(Action<AppOptions, string?> listener)
        {
            _onChange = listener;
            return null;
        }
    }
}
