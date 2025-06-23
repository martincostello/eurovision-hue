// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Spectre.Console;
using ConsoleColor = Spectre.Console.Color;

namespace MartinCostello.EurovisionHue;

internal static class IAnsiConsoleExtensions
{
    public static void WriteErrorLine(this IAnsiConsole console, string message)
    {
        console.MarkupLineInterpolated($"[{ConsoleColor.Red}]{Emoji.Known.CrossMark} {message}[/]");
    }
}
