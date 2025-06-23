// Copyright (c) Martin Costello, 2025. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Text.Json.Nodes;

namespace MartinCostello.EurovisionHue;

internal static class HueResponseBuilder
{
    public static JsonObject ForGetLights(params (Guid Id, string Name)[] lights)
    {
        var data = new JsonArray();

        foreach ((var id, var name) in lights)
        {
            data.Add(new JsonObject()
            {
                ["id"] = id,
                ["metadata"] = new JsonObject()
                {
                    ["name"] = name,
                },
            });
        }

        return new JsonObject()
        {
            ["errors"] = new JsonArray(),
            ["data"] = data,
        };
    }

    public static JsonObject ForPutLight()
        => new() { ["errors"] = new JsonArray() };
}
