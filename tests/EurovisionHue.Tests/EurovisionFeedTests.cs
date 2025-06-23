// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using Microsoft.Extensions.Options;
using Spectre.Console.Testing;

namespace MartinCostello.EurovisionHue;

public sealed class EurovisionFeedTests(ITestOutputHelper outputHelper) : IDisposable
{
    private readonly TestConsole _console = new();
    private readonly ITestOutputHelper _outputHelper = outputHelper;

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

        var target = new EurovisionFeed(Options.Create(options), _console, TimeProvider.System);

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

    public void Dispose()
    {
        if (_console is { })
        {
            _outputHelper.WriteLine(string.Empty);
            _outputHelper.WriteLine(_console.Output);
            _console.Dispose();
        }
    }
}
