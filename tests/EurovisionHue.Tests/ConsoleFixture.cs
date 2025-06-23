// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Spectre.Console;
using Spectre.Console.Testing;

namespace MartinCostello.EurovisionHue;

public sealed class ConsoleFixture(ITestOutputHelper outputHelper) : IDisposable
{
    private readonly TestConsole _console = new();
    private readonly ITestOutputHelper _outputHelper = outputHelper;
    private readonly TimeProvider _timeProvider = TimeProvider.System;

    public IAnsiConsole Console => _console;

    public TimeProvider TimeProvider => _timeProvider;

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
