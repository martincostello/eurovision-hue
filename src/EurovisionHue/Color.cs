// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.EurovisionHue;

internal sealed record Color(int R, int G, int B)
{
    public (double X, double Y) ToXY()
    {
        // See https://gist.github.com/popcorn245/30afa0f98eea1c2fd34d
        var r = GammaCorrection(R / 255.0);
        var g = GammaCorrection(G / 255.0);
        var b = GammaCorrection(B / 255.0);

        var x = (r * 0.649926) + (g * 0.103455) + (b * 0.197109);
        var y = (r * 0.234327) + (g * 0.743075) + (b * 0.022598);
        var z = (r * 0.000000) + (g * 0.053077) + (b * 1.035763);

        var total = x + y + z;

        return (x / total, y / total);

        static double GammaCorrection(double color)
            => color > 0.04045 ? Math.Pow((color + 0.055) / 1.055, 2.4) : color / 12.92;
    }
}
