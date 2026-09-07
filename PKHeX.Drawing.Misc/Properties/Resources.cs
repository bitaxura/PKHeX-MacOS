using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace PKHeX.Drawing.Misc.Properties;

public static class Resources
{
    private static readonly Assembly Assembly = typeof(Resources).Assembly;
    private static readonly ConcurrentDictionary<string, RawImage?> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static RawImage? GetObject(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        return Cache.GetOrAdd(name, static key =>
        {
            var stream = GetStream(key);
            if (stream == null)
                return null;

            using (stream)
            {
                return RawImage.FromStream(stream);
            }
        });
    }

    private static Stream? GetStream(string name)
    {
        string filename = name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? name : name + ".png";
        var stream = Assembly.GetManifestResourceStream(filename);
        if (stream != null)
            return stream;

        // Try replacing hyphen with underscore or vice versa
        if (filename.Contains('-'))
        {
            stream = Assembly.GetManifestResourceStream(filename.Replace('-', '_'));
            if (stream != null)
                return stream;
        }
        else if (filename.Contains('_'))
        {
            stream = Assembly.GetManifestResourceStream(filename.Replace('_', '-'));
            if (stream != null)
                return stream;
        }

        return null;
    }

    public static RawImage box_wp16xy => GetObject("box_wp16xy")!;
    public static RawImage box_wp01bdsp => GetObject("box_wp01bdsp")!;
    public static RawImage box_wp02bdsp => GetObject("box_wp02bdsp")!;

    public static RawImage tr_00 => GetObject("tr_00")!;
    public static RawImage Bag_Key => GetObject("Bag_Key")!;
    public static RawImage star => GetObject("star")!;
    public static RawImage flavorprofile => GetObject("flavorprofile")!;

    public static RawImage donut_uni491 => GetObject("donut_uni491")!;
    public static RawImage donut_uni383 => GetObject("donut_uni383")!;
    public static RawImage donut_uni382 => GetObject("donut_uni382")!;
    public static RawImage donut_uni384 => GetObject("donut_uni384")!;
    public static RawImage donut_uni807 => GetObject("donut_uni807")!;

    public static class ResourceManager
    {
        public static object? GetObject(string name) => Resources.GetObject(name);
    }
}
