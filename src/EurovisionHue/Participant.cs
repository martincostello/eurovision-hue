// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using SkiaSharp;

namespace MartinCostello.EurovisionHue;

internal sealed record Participant(
    string Id,
    string Name,
    string Emoji,
    IReadOnlyList<string>? AlternateNames = default) : IEquatable<Participant>
{
    public static readonly Participant Unknown = new("EUR", "Unknown", "🇪🇺");

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

            using var stream = type.Assembly.GetManifestResourceStream($"{type.Namespace}.Flags.{Id}.png");
            using var image = SKBitmap.Decode(stream);

            var histogram = new Dictionary<Color, int>();

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var pixel = image.GetPixel(x, y);

                    if (pixel.Alpha < 128 || (pixel.Red + pixel.Green + pixel.Blue == 0))
                    {
                        // Skip mostly transparent pixels and black pixels
                        continue;
                    }

                    var color = new Color(pixel.Red, pixel.Green, pixel.Blue);

                    if (!histogram.TryGetValue(color, out var value))
                    {
                        value = 0;
                    }

                    histogram[color] = value + 1;
                }
            }

            // Remove any colors that represent less than 1% of the total
            // pixels from compression artifacts or small details (like a crest).
            var totalPixels = histogram.Values.Sum();
            var threshold = totalPixels * 0.01;

            _colors = [.. histogram
                .Where((p) => p.Value >= threshold)
                .OrderByDescending((p) => p.Value)
                .Take(count)
                .Select((p) => p.Key)];
        }

        return _colors;
    }

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Id);

    public override string ToString() => Name;
}
