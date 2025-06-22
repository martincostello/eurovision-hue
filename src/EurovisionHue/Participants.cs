// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Buffers;

namespace MartinCostello.EurovisionHue;

internal static class Participants
{
    private static readonly Dictionary<string, Participant> _participants = new()
    {
        ["Albania"] = "ALB",
        ["Andorra"] = "AND",
        ["Armenia"] = "ARM",
        ["Australia"] = "AUS",
        ["Austria"] = "AUT",
        ["Azerbaijan"] = "AZE",
        ["Belarus"] = "BLR",
        ["Belgium"] = "BEL",
        ["Bosnia and Herzegovina"] = "BIH",
        ["Bulgaria"] = "BGR",
        ["Croatia"] = "HRV",
        ["Cyprus"] = "CYP",
        ["Czechia"] = "CZE",
        ["Denmark"] = "DNK",
        ["Estonia"] = "EST",
        ["Finland"] = "FIN",
        ["France"] = "FRA",
        ["Georgia"] = "GEO",
        ["Germany"] = "DEU",
        ["Greece"] = "GRC",
        ["Hungary"] = "HUN",
        ["Iceland"] = "ISL",
        ["Ireland"] = "IRL",
        ["Israel"] = "ISR",
        ["Italy"] = "ITA",
        ["Latvia"] = "LVA",
        ["Lithuania"] = "LTU",
        ["Luxembourg"] = "LUX",
        ["Malta"] = "MLT",
        ["Moldova"] = "MDA",
        ["Monaco"] = "MCO",
        ["Morocco"] = "MAR",
        ["Montenegro"] = "MNE",
        ["Netherlands"] = "NLD",
        ["North Macedonia"] = "MKD",
        ["Norway"] = "NOR",
        ["Poland"] = "POL",
        ["Portugal"] = "PRT",
        ["Romania"] = "ROU",
        ["Russia"] = "RUS",
        ["San Marino"] = "SMR",
        ["Serbia"] = "SRB",
        ["Slovakia"] = "SVK",
        ["Slovenia"] = "SVN",
        ["Spain"] = "ESP",
        ["Sweden"] = "SWE",
        ["Switzerland"] = "CHE",
        ["Turkey"] = "TUR",
        ["Turkiye"] = "TUR",
        ["Ukraine"] = "UKR",
        ["United Kingdom"] = "GBR",
    };

    private static readonly Dictionary<string, Participant>.AlternateLookup<ReadOnlySpan<char>> _lookup;
    private static readonly int _limit;
    private static readonly SearchValues<string> _needle;

    static Participants()
    {
        _lookup = _participants.GetAlternateLookup<ReadOnlySpan<char>>();
        _limit = _participants.Keys.MaxBy((p) => p.Length)!.Length;
        _needle = SearchValues.Create([.. _participants.Keys], StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryFind(ReadOnlySpan<char> content, out Participant? participant)
    {
        participant = null;

        if (content.IndexOfAny(_needle) is { } index && index >= 0)
        {
            content = content[index..];

            var length = 1;

            while (length <= _limit)
            {
                var key = content[..length++];

                if (_lookup.TryGetValue(key, out var value))
                {
                    participant = value;
                    return true;
                }
            }
        }

        return false;
    }
}
