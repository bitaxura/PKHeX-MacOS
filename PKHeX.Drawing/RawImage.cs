using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace PKHeX.Drawing;

/// <summary>
/// Platform-neutral 32-bit BGRA uncompressed image buffer.
/// Used as the intermediate representation for sprite generation and composition.
/// </summary>
public sealed class RawImage
{
    public int Width { get; }
    public int Height { get; }
    public int Stride => Width * 4;
    public byte[] Data { get; }

    public RawImage(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
        Data = new byte[width * height * 4];
    }

    public RawImage(int width, int height, byte[] data)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length != width * height * 4)
            throw new ArgumentException($"Data buffer length {data.Length} does not match dimensions {width}x{height}*4 ({width * height * 4})");

        Width = width;
        Height = height;
        Data = data;
    }

    public Span<byte> AsSpan() => Data.AsSpan();
    public Span<byte> AsSpan(int start, int length) => Data.AsSpan(start, length);
    public Span<int> AsIntSpan() => MemoryMarshal.Cast<byte, int>(Data.AsSpan());

    public RawImage Clone()
    {
        var copy = new byte[Data.Length];
        Buffer.BlockCopy(Data, 0, copy, 0, Data.Length);
        return new RawImage(Width, Height, copy);
    }

    public void Fill(Color c)
    {
        int colorValue = ColorToBgra(c);
        AsIntSpan().Fill(colorValue);
    }

    public void Clear() => Array.Clear(Data, 0, Data.Length);

    public static int ColorToBgra(Color c)
    {
        return (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
    }

    public static Color BgraToColor(int bgra)
    {
        byte b = (byte)(bgra & 0xFF);
        byte g = (byte)((bgra >> 8) & 0xFF);
        byte r = (byte)((bgra >> 16) & 0xFF);
        byte a = (byte)((bgra >> 24) & 0xFF);
        return Color.FromArgb(a, r, g, b);
    }

    /// <summary>
    /// Decodes an encoded image (e.g. PNG) from a stream into a 32-bit BGRA RawImage.
    /// </summary>
    public static RawImage FromStream(Stream stream)
    {
        using var skBitmap = SKBitmap.Decode(stream) 
            ?? throw new InvalidOperationException("Failed to decode image stream.");

        var info = new SKImageInfo(skBitmap.Width, skBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var converted = new SKBitmap(info);
        skBitmap.CopyTo(converted, SKColorType.Bgra8888);

        var bytes = converted.Bytes;
        return new RawImage(converted.Width, converted.Height, bytes);
    }

    /// <summary>
    /// Decodes an encoded image from a byte span into a 32-bit BGRA RawImage.
    /// </summary>
    public static RawImage FromBytes(ReadOnlySpan<byte> bytes)
    {
        using var skBitmap = SKBitmap.Decode(bytes) 
            ?? throw new InvalidOperationException("Failed to decode image bytes.");

        var info = new SKImageInfo(skBitmap.Width, skBitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var converted = new SKBitmap(info);
        skBitmap.CopyTo(converted, SKColorType.Bgra8888);

        return new RawImage(converted.Width, converted.Height, converted.Bytes);
    }

    /// <summary>
    /// Decodes an image from a file into a 32-bit BGRA RawImage.
    /// </summary>
    public static RawImage FromFile(string path)
    {
        using var stream = File.OpenRead(path);
        return FromStream(stream);
    }

    /// <summary>
    /// Encodes this RawImage into PNG format and writes it to the destination stream.
    /// </summary>
    public void SavePng(Stream destination)
    {
        var info = new SKImageInfo(Width, Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var skBitmap = new SKBitmap();
        skBitmap.InstallPixels(info, GCHandle.Alloc(Data, GCHandleType.Pinned).AddrOfPinnedObject());
        using var image = SKImage.FromBitmap(skBitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(destination);
    }

    /// <summary>
    /// Encodes this RawImage into PNG format and saves it to a file.
    /// </summary>
    public void SavePng(string destinationPath)
    {
        using var fileStream = File.Create(destinationPath);
        SavePng(fileStream);
    }
}
