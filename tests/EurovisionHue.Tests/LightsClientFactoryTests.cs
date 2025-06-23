// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using HueApi.BridgeLocator;
using HueApi.Models.Clip;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace MartinCostello.EurovisionHue;

public sealed class LightsClientFactoryTests(ITestOutputHelper outputHelper) : IDisposable
{
    private readonly AppFixture _fixture = new(outputHelper);

    [Fact]
    public async Task Returns_Null_When_No_Bridges_Found()
    {
        // Arrange
        using var client = new HttpClient();

        var options = Options.Create(new AppOptions());
        var factory = new MockLightsClientFactory([], client, _fixture.Console, options);

        // Act
        var actual = await factory.CreateAsync(_fixture.CancellationToken);

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public async Task Returns_Null_When_One_Bridge_Found_But_Not_Confirmed_By_User()
    {
        // Arrange
        _fixture.Console.Input.PushTextWithEnter("n");

        var options = Options.Create(new AppOptions()
        {
            HueToken = string.Empty,
        });

        using var client = new HttpClient();

        var factory = new MockLightsClientFactory(["192.168.0.1"], client, _fixture.Console, options)
        {
            Token = Guid.NewGuid().ToString(),
        };

        // Act
        var actual = await factory.CreateAsync(_fixture.CancellationToken);

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public async Task Returns_Client_When_One_Bridge_Found()
    {
        // Arrange
        _fixture.Console.Input.PushTextWithEnter("y");

        var tempFile = Path.GetTempFileName();

        var options = Options.Create(new AppOptions()
        {
            HueToken = string.Empty,
            UserSettings = tempFile,
        });

        using var client = new HttpClient();

        var factory = new MockLightsClientFactory(["192.168.0.1"], client, _fixture.Console, options)
        {
            Token = Guid.NewGuid().ToString(),
        };

        // Act
        var actual = await factory.CreateAsync(_fixture.CancellationToken);

        // Assert
        actual.ShouldNotBeNull();

        File.Exists(tempFile).ShouldBeTrue();

        using var stream = File.OpenRead(tempFile);
        var settings = await JsonObject.ParseAsync(stream, cancellationToken: _fixture.CancellationToken);

        settings.ShouldNotBeNull();
        settings!["HueToken"]!.GetValue<string>().ShouldBe(factory.Token);
    }

    public void Dispose() => _fixture.Dispose();

    private sealed class MockLightsClientFactory(
        IList<string> bridges,
        HttpClient client,
        IAnsiConsole console,
        IOptions<AppOptions> options) : LightsClientFactory(client, console, options)
    {
        public string Token { get; set; } = "not-a-real-token";

        protected override Task<IList<LocatedBridge>> DiscoverAsync(TimeSpan fastTimeout, TimeSpan maxTimeout)
        {
            var result = new List<LocatedBridge>();

            foreach (var ip in bridges)
            {
                result.Add(new(Guid.NewGuid().ToString(), ip, null));
            }

            return Task.FromResult<IList<LocatedBridge>>(result);
        }

        protected override Task<RegisterEntertainmentResult?> RegisterAsync(string ip)
        {
            var result = new RegisterEntertainmentResult() { Username = Token };
            return Task.FromResult<RegisterEntertainmentResult?>(result);
        }
    }
}
