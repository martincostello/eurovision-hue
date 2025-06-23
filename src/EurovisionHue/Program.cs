// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

// Based on https://github.com/cnorthwood/eurovisionhue
using MartinCostello.EurovisionHue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;

    if (!cts.IsCancellationRequested)
    {
        cts.Cancel();
    }
};

var userSettings = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "MartinCostello",
    "EurovisionHue",
    "usersettings.json");

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile(userSettings, optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<App>()
    .Build();

var services = new ServiceCollection();

services.AddEurovisionHue(configuration);

services.AddOptions<AppOptions>()
        .PostConfigure((p) => p.UserSettings = userSettings);

using var provider = services.BuildServiceProvider();

var app = provider.GetRequiredService<App>();

return await app.RunAsync(cts.Token) ? 0 : 1;
