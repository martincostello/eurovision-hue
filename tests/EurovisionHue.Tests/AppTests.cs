// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

[Collection<BrowserFixture>]
public sealed class AppTests(
    BrowserFixture browser,
    ITestOutputHelper outputHelper) : IntegrationTests(browser, outputHelper)
{
    [Fact]
    public async Task Application_Runs_Successfully()
    {
        // Arrange
        var lightId = Guid.NewGuid();

        var options = new AppOptions()
        {
            ArticleSelector = Browser.ArticleSelector,
            FeedUrl = Browser.FeedUrl,
            HueToken = Application.HueToken,
            LightIds = [lightId],
        };

        var snapshot = Options.Create(options);

        var lights = HueResponseBuilder.ForGetLights(
        [
            (lightId, "My Lamp"),
            (Guid.NewGuid(), "Another Lamp"),
        ]);

        Application.RegisterGetLights(lights);
        Application.RegisterPutLight(lightId);

        using var client = Application.HttpInterception.CreateHttpClient();

        var clientFactory = new StaticLightsClientFactory(
            Application.BridgeIP,
            client,
            Application.Console,
            snapshot);

        var feed = new EurovisionFeed(
            Application.Console,
            Application.TimeProvider,
            snapshot);

        var target = new App(
            clientFactory,
            Application.Console,
            feed,
            snapshot);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, Application.CancellationToken);

        // Act
        var actual = await target.RunAsync(combined.Token);

        // Assert
        actual.ShouldBeTrue();
        Application.Console.Output.ShouldNotContain("exception", Case.Insensitive);
    }

    private sealed class StaticLightsClientFactory(
        string bridgeIP,
        HttpClient client,
        IAnsiConsole console,
        IOptions<AppOptions> options) : LightsClientFactory(client, console, options)
    {
        private readonly HttpClient _client = client;
        private readonly IAnsiConsole _console = console;
        private readonly AppOptions _options = options.Value;

        public override Task<LightsClient?> CreateAsync(CancellationToken cancellationToken)
        {
            var client = new LightsClient(bridgeIP, _options.HueToken, _client, _console);
            return Task.FromResult<LightsClient?>(client);
        }
    }
}
