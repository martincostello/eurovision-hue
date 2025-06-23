// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;
using JustEat.HttpClientInterception;
using Spectre.Console.Testing;

namespace MartinCostello.EurovisionHue;

public sealed class AppFixture(ITestOutputHelper outputHelper) : IDisposable
{
    private readonly TestConsole _console = new();
    private readonly HttpClientInterceptorOptions _interception = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();
    private readonly ITestOutputHelper _outputHelper = outputHelper;
    private readonly TimeProvider _timeProvider = TimeProvider.System;

    public string BridgeIP { get; } = "127.0.0.1";

    public CancellationToken CancellationToken { get; } = TestContext.Current.CancellationToken;

    public TestConsole Console => _console;

    public HttpClientInterceptorOptions HttpInterception => _interception;

    public string HueToken { get; } = "not-a-real-key";

    public TimeProvider TimeProvider => _timeProvider;

    public void Dispose()
    {
        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine(_console.Output);
        _console.Dispose();
    }

    public void RegisterGetLights(JsonObject response)
    {
        new HttpRequestInterceptionBuilder()
            .Requests()
            .ForUrl($"https://{BridgeIP}/clip/v2/resource/light")
            .ForGet()
            .ForRequestHeader("hue-application-key", HueToken)
            .Responds()
            .WithSystemTextJsonContent(response)
            .RegisterWith(HttpInterception);
    }

    public void RegisterPutLight(Guid id, Func<HttpContent, Task<bool>>? predicate = default)
    {
        var builder = new HttpRequestInterceptionBuilder()
            .Requests()
            .ForUrl($"https://{BridgeIP}/clip/v2/resource/light/{id}")
            .ForPut()
            .ForRequestHeader("hue-application-key", HueToken)
            .Responds()
            .WithSystemTextJsonContent(HueResponseBuilder.ForPutLight());

        if (predicate is not null)
        {
            builder.ForContent(predicate);
        }

        builder.RegisterWith(HttpInterception);
    }
}
