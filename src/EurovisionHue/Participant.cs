// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MartinCostello.EurovisionHue;

internal sealed record Participant(string Name) : IEquatable<Participant>
{
    private List<Rgba32>? _colors;

    public static implicit operator Participant(string name) => new(name);

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

        return string.Equals(Name, other.Name, StringComparison.Ordinal);
    }

    public IReadOnlyList<Rgba32> Colors(int count)
    {
        if (_colors is null)
        {
            var type = typeof(Participant);

            using var stream = type.Assembly.GetManifestResourceStream($"{type.Namespace}.Flags.{Name}.png")!;
            using var image = Image.Load<Rgba32>(stream);

            var histogram = new Dictionary<Rgba32, int>();

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var color = image[x, y];

                    if (color.A < 128 || (color.R + color.G + color.B == 0))
                    {
                        // Skip mostly transparent pixels and black pixels
                        continue;
                    }

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
        => Name?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;

    public override string ToString() => Name;
}
