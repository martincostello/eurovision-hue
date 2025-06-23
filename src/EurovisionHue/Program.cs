// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

if (args.SequenceEqual(["--install-deps"]))
{
    return Microsoft.Playwright.Program.Main(["install", "chromium", "--with-deps"]);
}

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;

    if (!cts.IsCancellationRequested)
    {
        cts.Cancel();
    }
};

return await MartinCostello.EurovisionHue.App.RunAsync((_) => { }, cts.Token);
