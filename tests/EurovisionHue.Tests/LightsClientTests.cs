// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using HueApi.Models;

namespace MartinCostello.EurovisionHue;

public sealed class LightsClientTests(ITestOutputHelper outputHelper) : IDisposable
{
    private readonly AppFixture _fixture = new(outputHelper);

    [Fact]
    public async Task Can_Get_Lights()
    {
        // Arrange
        var lights = HueResponseBuilder.ForGetLights(
        [
            (Guid.NewGuid(), "My Lamp"),
            (Guid.NewGuid(), "Another Lamp"),
        ]);

        _fixture.RegisterGetLights(lights);

        using var client = _fixture.HttpInterception.CreateHttpClient();

        var target = new LightsClient(
            _fixture.BridgeIP,
            _fixture.HueToken,
            client,
            _fixture.Console);

        // Act
        var actual = await target.GetLightsAsync();

        // Assert
        actual.ShouldNotBeEmpty();
        actual.ShouldAllBe((p) => p.Id != Guid.Empty);
        actual.ShouldAllBe((p) => p.Metadata != null);
        actual.Count.ShouldBe(2);
        actual.Select((p) => p.Metadata?.Name).ShouldBe(["Another Lamp", "My Lamp"]);

        _fixture.Console.Output.ShouldBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Can_Change_Light()
    {
        // Arrange
        var color = new Color(0, 36, 125);
        var light = new Light()
        {
            Id = Guid.NewGuid(),
            Metadata = new() { Name = "My Lamp" },
        };

        _fixture.RegisterPutLight(
            light.Id,
            async (content) =>
            {
                var request = JsonObject.Parse(await content.ReadAsStringAsync())!;

                return request["on"]!["on"]!.GetValue<bool>() &&
                       Math.Abs(request["color"]!["xy"]!["x"]!.GetValue<double>() - 0.15456155092290103) < double.Epsilon &&
                       Math.Abs(request["color"]!["xy"]!["y"]!.GetValue<double>() - 0.06491401900351479) < double.Epsilon &&
                       request["dynamics"]!["duration"]!.GetValue<int>() == 3_000;
            });

        using var client = _fixture.HttpInterception.CreateHttpClient();

        var target = new LightsClient(
            _fixture.BridgeIP,
            _fixture.HueToken,
            client,
            _fixture.Console);

        // Act and Assert
        await Should.NotThrowAsync(() => target.ChangeAsync(light, color));

        _fixture.Console.Output.ShouldBeNullOrWhiteSpace();
    }

    public void Dispose() => _fixture.Dispose();
}
