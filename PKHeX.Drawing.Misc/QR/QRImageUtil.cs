using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace PKHeX.Drawing.Misc;

/// <summary>
/// Provides utility methods for composing and extending QR code images with overlays and text.
/// </summary>
public static class QRImageUtil
{
    /// <summary>
    /// Creates a new QR image with a preview image layered in the center.
    /// </summary>
    /// <param name="qr">The base QR code image.</param>
    /// <param name="preview">The preview image to overlay.</param>
    /// <returns>A new RawImage with the preview image centered on the QR code.</returns>
    public static RawImage GetQRImage(RawImage qr, RawImage preview)
    {
        // create a small area with the pk sprite, with a white background
        var foreground = new RawImage(preview.Width + 4, preview.Height + 4);
        foreground.Fill(Color.White);
        int px = (foreground.Width / 2) - (preview.Width / 2);
        int py = (foreground.Height / 2) - (preview.Height / 2);
        ImageUtil.BlendOver(foreground, preview, px, py);

        // Layer on Preview Image
        int x = (qr.Width / 2) - (foreground.Width / 2);
        int y = (qr.Height / 2) - (foreground.Height / 2);
        return ImageUtil.LayerImage(qr, foreground, x, y);
    }

    /// <summary>
    /// Creates an extended QR image with additional text and formatting.
    /// </summary>
    public static RawImage GetQRImageExtended(RawImage qr, RawImage pk, int width, int height, ReadOnlySpan<string> lines, string extraText)
    {
        var pic = GetQRImage(qr, pk);
        return ExtendImage(qr, width, height, pic, lines, extraText);
    }

    /// <summary>
    /// Extends an image with additional lines of text and formatting.
    /// </summary>
    private static RawImage ExtendImage(RawImage qr, int width, int height, RawImage pic, ReadOnlySpan<string> lines, string extraText)
    {
        var newpic = new RawImage(width, height);
        newpic.Fill(Color.White);
        ImageUtil.BlendOver(newpic, pic, 0, 0);

        var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        var handle = GCHandle.Alloc(newpic.Data, GCHandleType.Pinned);
        try
        {
            using var surface = SKSurface.Create(info, handle.AddrOfPinnedObject(), width * 4);
            if (surface != null)
            {
                var canvas = surface.Canvas;
                using var font = new SKFont(SKTypeface.Default, 11);
                using var paint = new SKPaint(font)
                {
                    Color = SKColors.Black,
                    IsAntialias = true,
                };

                const int indent = 18;
                canvas.DrawText(GetLine(lines, 0), indent, qr.Height - 5, font, paint);
                canvas.DrawText(GetLine(lines, 1), indent, qr.Height + 8, font, paint);
                canvas.DrawText(GetLine2(lines), indent, qr.Height + 20, font, paint);
                canvas.DrawText(GetLine(lines, 3) + extraText, indent, qr.Height + 32, font, paint);
                canvas.Flush();
            }
        }
        finally
        {
            handle.Free();
        }

        return newpic;
    }

    /// <summary>
    /// Gets and formats the second line of text for display.
    /// </summary>
    private static string GetLine2(ReadOnlySpan<string> lines) => GetLine(lines, 2)
        .Replace(Environment.NewLine, "/")
        .Replace("//", "   ")
        .Replace(":/", ": ");

    /// <summary>
    /// Gets a specific line of text or an empty string if the line does not exist.
    /// </summary>
    private static string GetLine(ReadOnlySpan<string> lines, int line) => lines.Length <= line ? string.Empty : lines[line];
}
