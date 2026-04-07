using System.Drawing;
using QRCoder;

namespace OWTrackerDesktop.Services;

public static class DesktopConnectionQrBitmap
{
    /// <summary>PNG-backed bitmap for display; caller must dispose.</summary>
    public static Bitmap? Create(string connectionUri, int pixelsPerModule = 5)
    {
        if (string.IsNullOrEmpty(connectionUri))
            return null;

        try
        {
            using var gen = new QRCodeGenerator();
            using var data = gen.CreateQrCode(connectionUri, QRCodeGenerator.ECCLevel.Q);
            var pngQr = new PngByteQRCode(data);
            byte[] pngBytes = pngQr.GetGraphic(pixelsPerModule);
            using var ms = new MemoryStream(pngBytes);
            return new Bitmap(ms);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"QR render failed: {ex.Message}");
            return null;
        }
    }
}
