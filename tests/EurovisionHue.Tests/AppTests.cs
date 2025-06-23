// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Reflection;
using System.Text.Json.Nodes;
using JustEat.HttpClientInterception;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

public sealed class AppTests(ITestOutputHelper outputHelper) : IAsyncLifetime
{
    private readonly ConsoleFixture _fixture = new(outputHelper);

    [Fact]
    public async Task Application_Runs_Successfully()
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
            FeedUrl = new Uri($"file://{feedUrl}").ToString(),
            HueToken = "not-a-real-key",
            LightIds = [Guid.Parse("83b98620-2e23-47bc-9291-47e93e3c200c")],
        };

        var interception = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();

        var lights = new JsonObject()
        {
            ["errors"] = new JsonArray(),
            ["data"] = new JsonArray()
            {
                new JsonObject()
                {
                    ["id"] = "83b98620-2e23-47bc-9291-47e93e3c200c",
                    ["metadata"] = new JsonObject()
                    {
                        ["name"] = "My Lamp",
                    },
                },
                new JsonObject()
                {
                    ["id"] = "18731230-8d5e-4ebd-a6e2-9518438cbcec",
                    ["metadata"] = new JsonObject()
                    {
                        ["name"] = "Another Lamp",
                    },
                },
            },
        };

        new HttpRequestInterceptionBuilder()
            .Requests()
            .ForUrl($"https://127.0.0.1/clip/v2/resource/light")
            .ForGet()
            .ForRequestHeader("hue-application-key", options.HueToken)
            .Responds()
            .WithSystemTextJsonContent(lights)
            .RegisterWith(interception);

        var response = new JsonObject()
        {
            ["errors"] = new JsonArray(),
        };

        new HttpRequestInterceptionBuilder()
            .Requests()
            .ForUrl($"https://127.0.0.1/clip/v2/resource/light/{options.LightIds[0]}")
            .ForPut()
            .ForRequestHeader("hue-application-key", options.HueToken)
            .Responds()
            .WithSystemTextJsonContent(response)
            .RegisterWith(interception);

        using var client = interception.CreateHttpClient();

        var clientFactory = new StaticLightsClientFactory(
            Options.Create(options),
            _fixture.Console,
            client);

        var feed = new EurovisionFeed(
            Options.Create(options),
            _fixture.Console,
            _fixture.TimeProvider);

        var target = new App(
            clientFactory,
            _fixture.Console,
            feed,
            Options.Create(options));

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        // Act
        var actual = await target.RunAsync(cts.Token);

        // Assert
        actual.ShouldBeTrue();
    }

    public ValueTask InitializeAsync()
    {
        InstallPlaywright();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _fixture.Dispose();
        return ValueTask.CompletedTask;
    }

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(["install"]);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }

    private sealed class StaticLightsClientFactory(
        IOptions<AppOptions> options,
        IAnsiConsole console,
        HttpClient client) : LightsClientFactory(options, console, client)
    {
        private readonly HttpClient _client = client;
        private readonly IAnsiConsole _console = console;
        private readonly AppOptions _options = options.Value;

        public override Task<LightsClient?> CreateAsync(CancellationToken cancellationToken)
        {
            var client = new LightsClient("127.0.0.1", _options.HueToken, _client, _console);
            return Task.FromResult<LightsClient?>(client);
        }
    }
}
