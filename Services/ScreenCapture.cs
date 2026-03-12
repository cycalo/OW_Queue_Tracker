using System.Drawing;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace OWTrackerDesktop.Services;

public static class ScreenCapture
{
    /// <summary>
    /// Which screen to capture. Null = primary. Set from UI when user has multiple monitors.
    /// </summary>
    public static Screen? TargetScreen { get; set; }

    /// <summary>
    /// Top-center banner: SEARCHING / GAME FOUND
    /// </summary>
    public static Bitmap CaptureQueueBanner()
    {
        // Slightly larger capture area to better include queue-state text at different resolutions/scales.
        return CaptureRegionRelative(0.20, 0.0, 0.60, 0.10);
    }

    /// <summary>
    /// Center overlay: ENTERING PREGAME
    /// </summary>
    public static Bitmap CapturePreGameScreen()
    {
        return CaptureRegionRelative(0.34, 0.27, 0.33, 0.06);
    }

    private static Bitmap CaptureRegionRelative(
        double xPercent, double yPercent,
        double widthPercent, double heightPercent)
    {
        var screen = TargetScreen ?? Screen.PrimaryScreen!;
        var bounds = screen.Bounds;
        int x = bounds.X + (int)(bounds.Width * xPercent);
        int y = bounds.Y + (int)(bounds.Height * yPercent);
        int width = (int)(bounds.Width * widthPercent);
        int height = (int)(bounds.Height * heightPercent);

        return CaptureRegion(x, y, width, height);
    }

    private static Bitmap CaptureRegion(int x, int y, int width, int height)
    {
        var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);
        g.CopyFromScreen(x, y, 0, 0, new Size(width, height));
        return bitmap;
    }

    /// <summary>
    /// Convert System.Drawing.Bitmap to WinRT SoftwareBitmap for the OCR API.
    /// </summary>
    public static async Task<SoftwareBitmap> ConvertToSoftwareBitmap(Bitmap bitmap)
    {
        using var memoryStream = new MemoryStream();
        bitmap.Save(memoryStream, ImageFormat.Png);
        memoryStream.Position = 0;

        using var randomAccessStream = new InMemoryRandomAccessStream();
        await memoryStream.CopyToAsync(randomAccessStream.AsStreamForWrite());
        randomAccessStream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
        return await decoder.GetSoftwareBitmapAsync(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied);
    }
}
