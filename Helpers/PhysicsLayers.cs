using System.Collections.Generic;
using Godot;

namespace CookingSim.Helpers;

public static class PhysicsLayers
{
    private static readonly Dictionary<string, int> NAME_TO_LAYER = new();
    private static bool _initialized;

    private static void EnsureInit()
    {
        if (_initialized) return;

        for (int i = 1; i <= 32; i++)
        {
            string key = $"layer_names/3d_physics/layer_{i}";
            if (ProjectSettings.HasSetting(key))
            {
                string name = (string)ProjectSettings.GetSetting(key);
                if (!string.IsNullOrEmpty(name))
                {
                    NAME_TO_LAYER[name.ToLowerInvariant()] = i; // store 1-based layer number
                }
            }
        }

        _initialized = true;
    }

    public static int GetLayerNumber(string name)
    {
        EnsureInit();
        if (NAME_TO_LAYER.TryGetValue(name.ToLowerInvariant(), out int layer))
            return layer;

        GD.PrintErr($"Physics layer name not found: {name}");
        return 0;
    }

    public static int GetLayerBit(int layerNumber)
    {
        // layerNumber is 1-based; bit is 0-based
        if (layerNumber < 1 || layerNumber > 32)
        {
            GD.PrintErr($"Invalid physics layer number: {layerNumber}");
            return 0;
        }
        return 1 << (layerNumber - 1);
    }

    public static int GetLayerBit(string name)
    {
        int layer = GetLayerNumber(name);
        return layer == 0 ? 0 : GetLayerBit(layer);
    }
}