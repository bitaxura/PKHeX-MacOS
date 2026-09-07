using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PKHeX.Drawing;

namespace PKHeX.Avalonia;

public static class ImageBridge
{
    public static WriteableBitmap ToBitmap(this RawImage raw)
    {
        var wb = new WriteableBitmap(
            new PixelSize(raw.Width, raw.Height),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Unpremul);

        using var fb = wb.Lock();
        Marshal.Copy(raw.Data, 0, fb.Address, raw.Data.Length);
        return wb;
    }
}
