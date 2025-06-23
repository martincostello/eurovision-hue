// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MartinCostello.EurovisionHue;

internal sealed record Participant(
    string Id,
    string Name,
    string Emoji,
    IReadOnlyList<string>? AlternateNames = default) : IEquatable<Participant>
{
    private List<Color>? _colors;

    public IReadOnlyList<string> Names { get; } = [Name, .. AlternateNames ?? []];

    public bool Equals(Participant? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(Id, other.Id, StringComparison.Ordinal);
    }

    public IReadOnlyList<Color> Colors(int count)
    {
        if (_colors is null)
        {
            var type = typeof(Participant);

            using var stream = type.Assembly.GetManifestResourceStream($"{type.Namespace}.Flags.{Id}.png")!;
            using var image = Image.Load<Rgba32>(stream);

            var histogram = new Dictionary<Color, int>();

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image[x, y];

                    if (pixel.A < 128 || (pixel.R + pixel.G + pixel.B == 0))
                    {
                        // Skip mostly transparent pixels and black pixels
                        continue;
                    }

                    var color = new Color(pixel.R, pixel.G, pixel.B);

                    if (!histogram.TryGetValue(color, out var value))
                    {
                        value = 0;
                    }

                    histogram[color] = value + 1;
                }
            }

            _colors = [.. histogram
                .OrderByDescending((p) => p.Value)
                .Take(count)
                .Select((p) => p.Key)];
        }

        return _colors;
    }

    public override int GetHashCode()
        => Id?.GetHashCode(StringComparison.Ordinal) ?? 0;

    public override string ToString() => Name;
}
