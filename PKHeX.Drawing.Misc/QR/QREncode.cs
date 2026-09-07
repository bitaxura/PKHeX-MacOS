using System.Drawing;
using PKHeX.Core;
using QRCoder;

namespace PKHeX.Drawing.Misc;

/// <summary>
/// Provides methods for generating QR codes from various PKHeX data types.
/// </summary>
public static class QREncode
{
    /// <summary>
    /// Generates a QR code image from a <see cref="DataMysteryGift"/> object.
    /// </summary>
    /// <param name="mg">The mystery gift data to encode.</param>
    /// <returns>A RawImage containing the QR code.</returns>
    public static RawImage GenerateQRCode(DataMysteryGift mg) => GenerateQRCode(QRMessageUtil.GetMessage(mg));

    /// <summary>
    /// Generates a QR code image from a <see cref="PKM"/> object.
    /// </summary>
    /// <param name="pk">The PKM data to encode.</param>
    /// <returns>A RawImage containing the QR code.</returns>
    public static RawImage GenerateQRCode(PKM pk) => GenerateQRCode(QRMessageUtil.GetMessage(pk));

    /// <summary>
    /// Generates a QR code image for a Generation 7 PKM with additional options.
    /// </summary>
    /// <param name="pk7">The Generation 7 PKM data to encode.</param>
    /// <param name="box">The box number for the PKM.</param>
    /// <param name="slot">The slot number in the box.</param>
    /// <param name="copies">The number of copies to encode.</param>
    /// <returns>A RawImage containing the QR code.</returns>
    public static RawImage GenerateQRCode7(PK7 pk7, int box = 0, int slot = 0, int copies = 1)
        => GenerateQRCode(QRMessageUtil.GetMessage(pk7, box, slot, copies), ppm: 4);

    /// <summary>
    /// Generates a QR code RawImage from a message string.
    /// </summary>
    /// <param name="msg">The message to encode in the QR code.</param>
    /// <param name="ppm">Pixels per module for the QR code graphic.</param>
    /// <returns>A RawImage containing the QR code.</returns>
    private static RawImage GenerateQRCode(string msg, int ppm = 4)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(msg, QRCodeGenerator.ECCLevel.Q);
        var matrix = data.ModuleMatrix;
        int modules = matrix.Count;
        int quietZone = 4;
        int totalModules = modules + (quietZone * 2);
        int pixelSize = totalModules * ppm;
        var raw = new RawImage(pixelSize, pixelSize);
        raw.Fill(Color.White);

        int black = RawImage.ColorToBgra(Color.Black);
        var span = raw.AsIntSpan();

        for (int my = 0; my < modules; my++)
        {
            var row = matrix[my];
            int pyStart = (my + quietZone) * ppm;
            for (int mx = 0; mx < modules; mx++)
            {
                if (!row[mx])
                    continue;

                int pxStart = (mx + quietZone) * ppm;
                for (int dy = 0; dy < ppm; dy++)
                {
                    int rowOffset = (pyStart + dy) * pixelSize;
                    for (int dx = 0; dx < ppm; dx++)
                    {
                        span[rowOffset + pxStart + dx] = black;
                    }
                }
            }
        }
        return raw;
    }
}
