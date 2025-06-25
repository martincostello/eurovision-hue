// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using JustEat.HttpClientInterception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
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

        var lights = HueResponseBuilder.ForGetLights(
        [
            (lightId, "My Lamp"),
            (Guid.NewGuid(), "Another Lamp"),
        ]);

        Application.RegisterGetLights(lights);
        Application.RegisterPutLight(lightId);

        Application.RegisterGitHubGist(
            "5ac0a49cd687c073ec09f4172c37185f",
            Browser.FeedUrl,
            Browser.ArticleSelector);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        using var combined = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, Application.CancellationToken);

        // Act
        var actual = await App.RunAsync(
            (services) =>
            {
                services.AddSingleton(Application);
                services.AddSingleton<IAnsiConsole>(Application.Console);
                services.AddSingleton<LightsClientFactory, StaticLightsClientFactory>();

                services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                    (_) => new HttpRequestInterceptionFilter(Application.HttpInterception));

                services.AddOptions<AppOptions>().PostConfigure((options) =>
                {
                    options.HueToken = Application.HueToken;
                    options.LightIds = [lightId];
                    options.ReloadFrequency = TimeSpan.FromSeconds(1);
                });
            },
            combined.Token);

        // Assert
        actual.ShouldBe(0);
        Application.Console.Output.ShouldNotContain("exception", Case.Insensitive);
    }

    private sealed class HttpRequestInterceptionFilter(HttpClientInterceptorOptions options) : IHttpMessageHandlerBuilderFilter
    {
        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return (builder) =>
            {
                // Run any actions the application has configured for itself
                next(builder);

                // Add the interceptor as the last message handler
                builder.AdditionalHandlers.Add(options.CreateHttpMessageHandler());
            };
        }
    }

    private sealed class StaticLightsClientFactory(
        AppFixture fixture,
        HttpClient client,
        IAnsiConsole console,
        IOptions<AppOptions> options) : LightsClientFactory(client, console, options)
    {
        private readonly HttpClient _client = client;
        private readonly IAnsiConsole _console = console;
        private readonly AppOptions _options = options.Value;

        public override Task<LightsClient?> CreateAsync(CancellationToken cancellationToken)
        {
            var client = new LightsClient(fixture.BridgeIP, _options.HueToken, _client, _console);
            return Task.FromResult<LightsClient?>(client);
        }
    }
}
