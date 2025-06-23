// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using HueApi.Models;
using JustEat.HttpClientInterception;

namespace MartinCostello.EurovisionHue;

public sealed class LightsClientTests(ITestOutputHelper outputHelper) : IDisposable
{
    private readonly ConsoleFixture _fixture = new(outputHelper);

    [Fact]
    public async Task Can_Get_Lights()
    {
        // Arrange
        var ip = "127.0.0.1";
        var key = "not-a-real-key";

        var options = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();

        using var client = options.CreateHttpClient();

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
            .ForUrl("https://127.0.0.1/clip/v2/resource/light")
            .ForGet()
            .ForRequestHeader("hue-application-key", key)
            .Responds()
            .WithSystemTextJsonContent(lights)
            .RegisterWith(options);

        var target = new LightsClient(ip, key, client, _fixture.Console);

        // Act
        var actual = await target.GetLightsAsync();

        // Assert
        actual.ShouldNotBeEmpty();
        actual.ShouldAllBe((p) => p.Id != Guid.Empty);
        actual.ShouldAllBe((p) => p.Metadata != null);
        actual.Count.ShouldBe(2);
        actual.Select((p) => p.Metadata?.Name).ShouldBe(["Another Lamp", "My Lamp"]);
    }

    [Fact]
    public async Task Can_Change_Light()
    {
        // Arrange
        var ip = "127.0.0.1";
        var key = "not-a-real-key";

        var color = new Color(0, 36, 125);
        var light = new Light()
        {
            Id = Guid.NewGuid(),
            Metadata = new() { Name = "My Lamp" },
        };

        var options = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();

        using var client = options.CreateHttpClient();

        var response = new JsonObject()
        {
            ["errors"] = new JsonArray(),
        };

        new HttpRequestInterceptionBuilder()
            .Requests()
            .ForUrl($"https://127.0.0.1/clip/v2/resource/light/{light.Id}")
            .ForPut()
            .ForRequestHeader("hue-application-key", key)
            .ForContent(async (content) =>
            {
                var request = JsonObject.Parse(await content.ReadAsStringAsync())!;

                return request["on"]!["on"]!.GetValue<bool>() &&
                       request["color"]!["xy"]!["x"]!.GetValue<double>() == 0.15456155092290103 &&
                       request["color"]!["xy"]!["y"]!.GetValue<double>() == 0.06491401900351479 &&
                       request["dynamics"]!["duration"]!.GetValue<int>() == 3_000;
            })
            .Responds()
            .WithSystemTextJsonContent(response)
            .RegisterWith(options);

        var target = new LightsClient(ip, key, client, _fixture.Console);

        // Act and Assert
        await Should.NotThrowAsync(() => target.ChangeAsync(light, color));

        outputHelper.Output.ShouldBeNullOrWhiteSpace();
    }

    public void Dispose() => _fixture.Dispose();
}
