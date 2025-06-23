// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Buffers;

namespace MartinCostello.EurovisionHue;

internal static class Participants
{
    private static readonly List<Participant> _participants =
    [
        new("ALB", "Albania", "🇦🇱"),
        new("AND", "Andorra", "🇦🇩"),
        new("ARM", "Armenia", "🇦🇲"),
        new("AUS", "Australia", "🇦🇺"),
        new("AUT", "Austria", "🇦🇹"),
        new("AZE", "Azerbaijan", "🇦🇿"),
        new("BLR", "Belarus", "🇧🇾"),
        new("BEL", "Belgium", "🇧🇪"),
        new("BIH", "Bosnia and Herzegovina", "🇧🇦", ["Bosnia & Herzegovina"]),
        new("BGR", "Bulgaria", "🇧🇬"),
        new("HRV", "Croatia", "🇭🇷"),
        new("CYP", "Cyprus", "🇨🇾"),
        new("CZE", "Czechia", "🇨🇿"),
        new("DNK", "Denmark", "🇩🇰"),
        new("EST", "Estonia", "🇪🇪"),
        new("FIN", "Finland", "🇫🇮"),
        new("FRA", "France", "🇫🇷"),
        new("GEO", "Georgia", "🇬🇪"),
        new("DEU", "Germany", "🇩🇪"),
        new("GRC", "Greece", "🇬🇷"),
        new("HUN", "Hungary", "🇭🇺"),
        new("ISL", "Iceland", "🇮🇸"),
        new("IRL", "Ireland", "🇮🇪"),
        new("ISR", "Israel", "🇮🇱"),
        new("ITA", "Italy", "🇮🇹"),
        new("LVA", "Latvia", "🇱🇻"),
        new("LTU", "Lithuania", "🇱🇹"),
        new("LUX", "Luxembourg", "🇱🇺"),
        new("MLT", "Malta", "🇲🇹"),
        new("MDA", "Moldova", "🇲🇩"),
        new("MCO", "Monaco", "🇲🇨"),
        new("MAR", "Morocco", "🇲🇦"),
        new("MNE", "Montenegro", "🇲🇪"),
        new("NLD", "Netherlands", "🇳🇱"),
        new("MKD", "North Macedonia", "🇲🇰"),
        new("NOR", "Norway", "🇳🇴"),
        new("POL", "Poland", "🇵🇱"),
        new("PRT", "Portugal", "🇵🇹"),
        new("ROU", "Romania", "🇷🇴"),
        new("RUS", "Russia", "🇷🇺"),
        new("SMR", "San Marino", "🇸🇲"),
        new("SRB", "Serbia", "🇷🇸"),
        new("SVK", "Slovakia", "🇸🇰"),
        new("SVN", "Slovenia", "🇸🇮"),
        new("ESP", "Spain", "🇪🇸"),
        new("SWE", "Sweden", "🇸🇪"),
        new("CHE", "Switzerland", "🇨🇭"),
        new("TUR", "Turkey", "🇹🇷", ["Turkiye", "Türkiye"]),
        new("UKR", "Ukraine", "🇺🇦"),
        new("GBR", "United Kingdom", "🇬🇧"),
    ];

    private static readonly Dictionary<string, Participant> _names;
    private static readonly Dictionary<string, Participant>.AlternateLookup<ReadOnlySpan<char>> _lookup;
    private static readonly int _limit;
    private static readonly SearchValues<string> _needle;

    static Participants()
    {
        _names = new(StringComparer.OrdinalIgnoreCase);

        foreach (var participant in _participants)
        {
            foreach (var name in participant.Names)
            {
                _names[name] = participant;
            }
        }

        _lookup = _names.GetAlternateLookup<ReadOnlySpan<char>>();
        _limit = _names.Keys.MaxBy((p) => p.Length)!.Length;
        _needle = SearchValues.Create([.. _names.Keys], StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryFind(ReadOnlySpan<char> content, out Participant? participant)
    {
        participant = null;

        if (content.IndexOfAny(_needle) is { } index && index > -1)
        {
            content = content[index..];

            var length = 1;

            while (length <= _limit)
            {
                var key = content[..length++];

                if (_lookup.TryGetValue(key, out participant))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
