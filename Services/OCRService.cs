using System.Drawing;
using Windows.Globalization;
using Windows.Media.Ocr;
using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

public class OCRService
{
    private readonly OcrEngine _ocrEngine;

    public OCRService()
    {
        _ocrEngine = OcrEngine.TryCreateFromLanguage(new Language("en"))
            ?? throw new InvalidOperationException(
                "Failed to initialize OCR engine. Ensure the English language pack is installed.");
    }

    public async Task<string> ExtractText(Bitmap bitmap)
    {
        try
        {
            var softwareBitmap = await ScreenCapture.ConvertToSoftwareBitmap(bitmap);
            var result = await _ocrEngine.RecognizeAsync(softwareBitmap);
            return result.Text;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OCR Error: {ex.Message}");
            return string.Empty;
        }
    }

    public static GameState DetectState(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return GameState.Idle;

        string normalized = NormalizeForDetection(text);

        if (ContainsAny(normalized, "gamefound", "matchfound"))
            return GameState.GameFound;

        if (ContainsAny(normalized, "enteringpregame", "pregame", "assemblingheroes"))
            return GameState.MatchStarting;

        bool hasSearchingText = ContainsAny(normalized, "searchingforgame", "searching", "searchforgame", "search");
        bool hasQueueContext = ContainsAny(
            normalized,
            "rolequeue",
            "openqueue",
            "cancelsearch",
            "estimatedtime",
            "timeelapsed");

        if (hasSearchingText || hasQueueContext)
            return GameState.Searching;

        return GameState.Idle;
    }

    private static string NormalizeForDetection(string text)
    {
        var chars = new char[text.Length];
        int index = 0;

        foreach (char ch in text)
        {
            if (char.IsLetterOrDigit(ch))
            {
                chars[index++] = char.ToLowerInvariant(ch);
            }
        }

        return new string(chars, 0, index);
    }

    private static bool ContainsAny(string text, params string[] tokens)
    {
        foreach (var token in tokens)
        {
            if (text.Contains(token, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public async Task<GameState> DetectCurrentState()
    {
        using var bannerCapture = ScreenCapture.CaptureQueueBanner();
        string bannerText = await ExtractText(bannerCapture);
        var state = DetectState(bannerText);

        if (state != GameState.Idle)
            return state;

        using var preGameCapture = ScreenCapture.CapturePreGameScreen();
        string preGameText = await ExtractText(preGameCapture);
        state = DetectState(preGameText);

        return state;
    }
}
