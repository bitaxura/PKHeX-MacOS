using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace PKHeX.Drawing.PokeSprite.Properties;

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

    public static RawImage b_unknown => GetObject("b_unknown")!;
    public static RawImage b_egg => GetObject("b_egg")!;
    public static RawImage b_490_e => GetObject("b_490-e") ?? GetObject("b_490_e")!;
    public static RawImage b_0 => GetObject("b_0")!;
    public static RawImage bitem_unk => GetObject("bitem_unk")!;
    public static RawImage bitem_tm => GetObject("bitem_tm")!;
    public static RawImage bitem_tr => GetObject("bitem_tr")!;
    public static RawImage b_249x => GetObject("b_249x")!;

    public static RawImage a_unknown => GetObject("a_unknown") ?? b_unknown;
    public static RawImage a_egg => GetObject("a_egg") ?? b_egg;
    public static RawImage a_490_e => GetObject("a_490-e") ?? GetObject("a_490_e") ?? b_490_e;
    public static RawImage aitem_tm => GetObject("aitem_tm") ?? bitem_tm;

    public static RawImage slotHover68 => GetObject("slotHover68")!;
    public static RawImage slotView68 => GetObject("slotView68")!;
    public static RawImage slotSet68 => GetObject("slotSet68")!;
    public static RawImage slotDel68 => GetObject("slotDel68")!;
    public static RawImage slotTrans68 => GetObject("slotTrans68")!;
    public static RawImage slotDrag68 => GetObject("slotDrag68")!;

    public static RawImage rare_icon_alt => GetObject("rare_icon_alt")!;
    public static RawImage rare_icon_alt_2 => GetObject("rare_icon_alt_2")!;
    public static RawImage dyna => GetObject("dyna")!;
    public static RawImage alpha_alt => GetObject("alpha_alt")!;
    public static RawImage warn => GetObject("warn")!;
    public static RawImage valid => GetObject("valid")!;
    public static RawImage hint => GetObject("hint")!;

    public static RawImage team => GetObject("team")!;
    public static RawImage locked => GetObject("locked")!;
    public static RawImage starter => GetObject("starter")!;

    public static RawImage _ball4 => GetObject("_ball4")!;

    public static RawImage party1 => GetObject("party1")!;
    public static RawImage party2 => GetObject("party2")!;
    public static RawImage party3 => GetObject("party3")!;
    public static RawImage party4 => GetObject("party4")!;
    public static RawImage party5 => GetObject("party5")!;
    public static RawImage party6 => GetObject("party6")!;

    public static RawImage sicksleep => GetObject("sicksleep")!;
    public static RawImage sicktoxic => GetObject("sicktoxic")!;
    public static RawImage sickpoison => GetObject("sickpoison")!;
    public static RawImage sickburn => GetObject("sickburn")!;
    public static RawImage sickparalyze => GetObject("sickparalyze")!;
    public static RawImage sickfrostbite => GetObject("sickfrostbite")!;

    public static class ResourceManager
    {
        public static object? GetObject(string name) => Resources.GetObject(name);
    }
}
