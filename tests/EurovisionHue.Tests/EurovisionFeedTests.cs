// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.Options;

namespace MartinCostello.EurovisionHue;

public sealed class EurovisionFeedTests(ITestOutputHelper outputHelper) : IDisposable
{
    private readonly ConsoleFixture _fixture = new(outputHelper);

    [Fact]
    public async Task Can_Find_Participants()
    {
        // Arrange
        var solutionRoot = typeof(EurovisionFeedTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "SolutionRoot")
            .Value!;

        var feedUrl = Path.Combine(solutionRoot, "demo.html");

        var options = new AppOptions()
        {
            ArticleSelector = "#participantsTable > tbody > tr > td:nth-child(2)",
            FeedFrequency = TimeSpan.FromSeconds(5),
            FeedUrl = new Uri(feedUrl).LocalPath,
        };

        var target = new EurovisionFeed(Options.Create(options), _fixture.Console, _fixture.TimeProvider);

        var actual = new List<Participant>();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        await foreach (var participant in target.ParticipantsAsync(cts.Token).WithCancellation(cts.Token))
        {
            actual.Add(participant);
        }

        // Assert
        actual.ShouldNotBeEmpty();
        actual.Distinct().Count().ShouldBeGreaterThan(1);
    }

    public void Dispose() => _fixture.Dispose();
}
