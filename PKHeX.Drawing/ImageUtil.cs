using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PKHeX.Drawing;

/// <summary>
/// Image Layering/Blending Utility operating on platform-neutral <see cref="RawImage"/> buffers.
/// </summary>
public static class ImageUtil
{
    public static RawImage LayerImage(RawImage baseLayer, RawImage overLayer, int x, int y, double transparency)
    {
        overLayer = CopyChangeOpacity(overLayer, transparency);
        return LayerImage(baseLayer, overLayer, x, y);
    }

    public static RawImage LayerImage(RawImage baseLayer, RawImage overLayer, int x, int y)
    {
        var result = baseLayer.Clone();
        BlendOver(result, overLayer, x, y);
        return result;
    }

    /// <summary>
    /// Blends <paramref name="src"/> over <paramref name="dst"/> at position (<paramref name="startX"/>, <paramref name="startY"/>) in-place.
    /// </summary>
    public static void BlendOver(RawImage dst, RawImage src, int startX, int startY)
    {
        int dstWidth = dst.Width;
        int dstHeight = dst.Height;
        int srcWidth = src.Width;
        int srcHeight = src.Height;

        var dstSpan = dst.AsSpan();
        var srcSpan = src.AsSpan();

        int xMin = Math.Max(0, -startX);
        int yMin = Math.Max(0, -startY);
        int xMax = Math.Min(srcWidth, dstWidth - startX);
        int yMax = Math.Min(srcHeight, dstHeight - startY);

        for (int sy = yMin; sy < yMax; sy++)
        {
            int dy = startY + sy;
            int srcRowOffset = sy * srcWidth * 4;
            int dstRowOffset = dy * dstWidth * 4;

            for (int sx = xMin; sx < xMax; sx++)
            {
                int dx = startX + sx;
                int sIdx = srcRowOffset + (sx * 4);
                int dIdx = dstRowOffset + (dx * 4);

                byte sa = srcSpan[sIdx + 3];
                if (sa == 0)
                    continue;

                if (sa == 255)
                {
                    dstSpan[dIdx + 0] = srcSpan[sIdx + 0];
                    dstSpan[dIdx + 1] = srcSpan[sIdx + 1];
                    dstSpan[dIdx + 2] = srcSpan[sIdx + 2];
                    dstSpan[dIdx + 3] = 255;
                    continue;
                }

                byte da = dstSpan[dIdx + 3];
                if (da == 0)
                {
                    dstSpan[dIdx + 0] = srcSpan[sIdx + 0];
                    dstSpan[dIdx + 1] = srcSpan[sIdx + 1];
                    dstSpan[dIdx + 2] = srcSpan[sIdx + 2];
                    dstSpan[dIdx + 3] = sa;
                    continue;
                }

                // Unpremultiplied "Over" compositing
                float sAlpha = sa / 255f;
                float dAlpha = (da / 255f) * (1f - sAlpha);
                float outAlpha = sAlpha + dAlpha;

                if (outAlpha > 0f)
                {
                    dstSpan[dIdx + 0] = (byte)Math.Clamp(((srcSpan[sIdx + 0] * sAlpha) + (dstSpan[dIdx + 0] * dAlpha)) / outAlpha, 0, 255);
                    dstSpan[dIdx + 1] = (byte)Math.Clamp(((srcSpan[sIdx + 1] * sAlpha) + (dstSpan[dIdx + 1] * dAlpha)) / outAlpha, 0, 255);
                    dstSpan[dIdx + 2] = (byte)Math.Clamp(((srcSpan[sIdx + 2] * sAlpha) + (dstSpan[dIdx + 2] * dAlpha)) / outAlpha, 0, 255);
                    dstSpan[dIdx + 3] = (byte)Math.Clamp(outAlpha * 255f, 0, 255);
                }
            }
        }
    }

    public static RawImage CopyChangeOpacity(RawImage img, double trans)
    {
        var copy = img.Clone();
        copy.ChangeOpacity(trans);
        return copy;
    }

    public static RawImage CopyChangeAllColorTo(RawImage img, Color c)
    {
        var copy = img.Clone();
        copy.ChangeAllColorTo(c);
        return copy;
    }

    public static RawImage CopyChangeTransparentTo(RawImage img, Color c, byte trans, int start = 0, int end = -1)
    {
        var copy = img.Clone();
        copy.ChangeTransparentTo(c, trans, start, end);
        return copy;
    }

    public static RawImage CopyWritePixels(RawImage img, Color c, int start, int end)
    {
        var copy = img.Clone();
        copy.WritePixels(c, start, end);
        return copy;
    }

    public static void ToGrayscale(this RawImage img, float intensity)
    {
        if (intensity is <= 0.01f or > 1f)
            return;

        SetAllColorToGrayScale(img.AsSpan(), intensity);
    }

    public static void ChangeOpacity(this RawImage img, double trans)
    {
        if (trans is <= 0.01f or > 1f)
            return;

        SetAllTransparencyTo(img.AsSpan(), trans);
    }

    public static void BlendTransparentTo(this RawImage img, Color c, byte trans, int start = 0, int end = -1)
    {
        var data = img.AsSpan();
        if (end == -1)
            end = data.Length;
        BlendAllTransparencyTo(data[start..end], c, trans);
    }

    public static void ChangeAllColorTo(this RawImage img, Color c)
    {
        ChangeAllColorTo(img.AsSpan(), c);
    }

    public static void ChangeTransparentTo(this RawImage img, Color c, byte trans, int start = 0, int end = -1)
    {
        var data = img.AsSpan();
        if (end == -1)
            end = data.Length;
        SetAllTransparencyTo(data[start..end], c, trans);
    }

    public static void WritePixels(this RawImage img, Color c, int start, int end)
    {
        ChangeAllTo(img.AsSpan(), c, start, end);
    }

    public static int GetAverageColor(this RawImage img)
    {
        return GetAverageColor(img.AsSpan());
    }

    public static void SetAllUsedPixelsOpaque(Span<byte> data, byte threshold = TransparencyThresholdHalf)
    {
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            if (data[i + 3] >= threshold)
                data[i + 3] = 0xFF;
        }
    }

    public static void RemovePixels(Span<byte> pixels, ReadOnlySpan<byte> original, byte threshold = TransparencyThresholdHalf)
    {
        var arr = MemoryMarshal.Cast<byte, int>(pixels);
        for (int i = original.Length - 4; i >= 0; i -= 4)
        {
            if (original[i + 3] >= threshold)
                arr[i >> 2] = 0;
        }
    }

    private static void SetAllTransparencyTo(Span<byte> data, double trans)
    {
        for (int i = data.Length - 4; i >= 0; i -= 4)
            data[i + 3] = (byte)(data[i + 3] * trans);
    }

    private static void SetAllTransparencyTo(Span<byte> data, Color c, byte trans, byte threshold = TransparencyThresholdHalf)
    {
        var arr = MemoryMarshal.Cast<byte, int>(data);
        var value = Color.FromArgb(trans, c).ToArgb();
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            if (data[i + 3] < threshold)
                arr[i >> 2] = value;
        }
    }

    private static void BlendAllTransparencyTo(Span<byte> data, Color c, byte trans, byte threshold = TransparencyThresholdHalf)
    {
        var arr = MemoryMarshal.Cast<byte, int>(data);
        var value = Color.FromArgb(trans, c).ToArgb();
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            var alpha = data[i + 3];
            if (alpha < threshold)
                arr[i >> 2] = value;
            else if (alpha != 0xFF)
                arr[i >> 2] = BlendColor(arr[i >> 2], value);
        }
    }

    private static int GetAverageColor(Span<byte> data, byte threshold = TransparencyThreshold16)
    {
        long r = 0, g = 0, b = 0;
        int count = 0;
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            var alpha = data[i + 3];
            if (alpha < threshold)
                continue;
            r += data[i + 2];
            g += data[i + 1];
            b += data[i + 0];
            count++;
        }
        if (count == 0)
            return 0;
        byte R = (byte)(r / count);
        byte G = (byte)(g / count);
        byte B = (byte)(b / count);
        return (0xFF << 24) | (R << 16) | (G << 8) | B;
    }

    private static int BlendColor(int color1, int color2, double amount = 0.2)
    {
        var a1 = (color1 >> 24) & 0xFF;
        var r1 = (color1 >> 16) & 0xFF;
        var g1 = (color1 >> 8) & 0xFF;
        var b1 = color1 & 0xFF;

        var a2 = (color2 >> 24) & 0xFF;
        var r2 = (color2 >> 16) & 0xFF;
        var g2 = (color2 >> 8) & 0xFF;
        var b2 = color2 & 0xFF;

        byte a = (byte)((a1 * amount) + (a2 * (1 - amount)));
        byte r = (byte)((r1 * amount) + (r2 * (1 - amount)));
        byte g = (byte)((g1 * amount) + (g2 * (1 - amount)));
        byte b = (byte)((b1 * amount) + (b2 * (1 - amount)));

        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    private static void ChangeAllTo(Span<byte> data, Color c, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, int>(data[start..end]);
        var value = Color.FromArgb(c.A, c.R, c.G, c.B).ToArgb();
        arr.Fill(value);
    }

    public static void ChangeAllColorTo(Span<byte> data, Color c)
    {
        byte R = c.R;
        byte G = c.G;
        byte B = c.B;
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            if (data[i + 3] == 0)
                continue;
            data[i + 0] = B;
            data[i + 1] = G;
            data[i + 2] = R;
        }
    }

    private static void SetAllColorToGrayScale(Span<byte> data, float intensity)
    {
        if (intensity <= 0f)
            return;

        if (intensity >= 0.999f)
        {
            SetAllColorToGrayScale(data);
            return;
        }

        float inverse = 1f - intensity;
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            if (data[i + 3] == 0)
                continue;
            byte greyS = (byte)((0.3 * data[i + 2]) + (0.59 * data[i + 1]) + (0.11 * data[i + 0]));
            data[i + 0] = (byte)((data[i + 0] * inverse) + (greyS * intensity));
            data[i + 1] = (byte)((data[i + 1] * inverse) + (greyS * intensity));
            data[i + 2] = (byte)((data[i + 2] * inverse) + (greyS * intensity));
        }
    }

    private static void SetAllColorToGrayScale(Span<byte> data)
    {
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            if (data[i + 3] == 0)
                continue;
            byte greyS = (byte)((0.3 * data[i + 2]) + (0.59 * data[i + 1]) + (0.11 * data[i + 0]));
            data[i + 0] = greyS;
            data[i + 1] = greyS;
            data[i + 2] = greyS;
        }
    }

    public static void GlowEdges(Span<byte> data, byte blue, byte green, byte red, int width, int reach = 3, double amount = 0.0777)
    {
        for (int i = data.Length - 4; i >= 0; i -= 4)
            data[i + PollutePixelColorIndex] = 0;
        PollutePixels(data, width, reach, amount);
        CleanPollutedPixels(data, blue, green, red);
    }

    private const int PollutePixelColorIndex = 0;
    private const byte TransparencyThresholdHalf = 0x80;
    private const byte TransparencyThreshold16 = 0x10;

    private static void PollutePixels(Span<byte> data, int width, int reach, double amount, byte threshold = TransparencyThresholdHalf)
    {
        int stride = width * 4;
        int height = data.Length / stride;
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            if (data[i + 3] < threshold)
                continue;

            int x = (i % stride) / 4;
            int y = (i / stride);
            int left = Math.Max(0, x - reach);
            int right = Math.Min(width - 1, x + reach);
            int top = Math.Max(0, y - reach);
            int bottom = Math.Min(height - 1, y + reach);
            for (int ix = left; ix <= right; ix++)
            {
                for (int iy = top; iy <= bottom; iy++)
                {
                    var c = 4 * (ix + (iy * width));
                    ref var b = ref data[c + PollutePixelColorIndex];
                    b += (byte)(amount * (0xFF - b));
                }
            }
        }
    }

    private static void CleanPollutedPixels(Span<byte> data, byte blue, byte green, byte red)
    {
        for (int i = data.Length - 4; i >= 0; i -= 4)
        {
            var transparency = data[i + PollutePixelColorIndex];
            if (transparency == 0)
                continue;

            data[i + 0] = blue;
            data[i + 1] = green;
            data[i + 2] = red;
            data[i + 3] = transparency;
        }
    }
}
