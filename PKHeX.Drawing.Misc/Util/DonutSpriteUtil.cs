using PKHeX.Core;
using PKHeX.Drawing.Misc.Properties;
using System;
using System.Drawing;

namespace PKHeX.Drawing.Misc;

/// <summary>
/// Provides utility methods for retrieving and composing sprites for Donuts.
/// </summary>
public static class DonutSpriteUtil
{
    /// <summary>
    /// Gets the sprite image for the specified <see cref="Donut9a"/>.
    /// </summary>
    /// <param name="donut">The donut to get the sprite for.</param>
    /// <returns>A <see cref="Bitmap"/> representing the sprite image.</returns>
    public static RawImage? Sprite(this Donut9a donut) => GetDonutImage(donut);
    public static RawImage? StarSprite => (RawImage?)Resources.ResourceManager.GetObject("star");
    public static RawImage? GetDonutFlavorImage(string donut) => (RawImage?)Resources.ResourceManager.GetObject(donut);
    public static RawImage? GetFlavorProfileImage() => (RawImage?)Resources.ResourceManager.GetObject("flavorprofile");

    private static RawImage? GetDonutImage(Donut9a donut)
    {
        if (donut.Donut is >= 198 and <= 202)
            return GetSpecialDonutImage(donut);

        ReadOnlySpan<string> flavors = ["sweet", "spicy", "sour", "bitter", "fresh", "mix"];
        var variant = donut.Donut % 6;
        var stars = donut.Stars;
        var flavor = flavors[variant];
        var resource = $"donut_{flavor}{stars:00}";
        return (RawImage?)Resources.ResourceManager.GetObject(resource);
    }

    private static RawImage? GetSpecialDonutImage(Donut9a donut) => donut.Donut switch
    {
        198 => Resources.donut_uni491, // Bad Dreams Cruller
        199 => Resources.donut_uni383, // Omega Old-Fashioned Donut
        200 => Resources.donut_uni382, // Alpha Old-Fashioned Donut
        201 => Resources.donut_uni384, // Delta Old-Fashioned Donut
        202 => Resources.donut_uni807, // Plasma-Glazed Donut
        _ => null,
    };
}
