using System.Drawing;
using Windows.Globalization;
using Windows.Media.Ocr;
using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

public class OCRService
{
    private OcrEngine _ocrEngine;
    private GameLanguage _language;

    public GameLanguage Language => _language;

    public OCRService(GameLanguage? language = null)
    {
        _language = language ?? GameLanguageStore.LoadOrDefault();
        _ocrEngine = TryCreateOcrEngine(_language.OcrLanguageTag, out _)
            ?? TryCreateOcrEngine(GameLanguageCatalog.Default.OcrLanguageTag, out _)
            ?? throw new InvalidOperationException(
                "Failed to initialize OCR. Install the English OCR language pack in Windows Settings " +
                "(Time & language → Language & region).");
    }

    public void SetLanguage(GameLanguage language)
    {
        var engine = TryCreateOcrEngine(language.OcrLanguageTag, out var resolvedTag);
        if (engine == null)
            throw new InvalidOperationException(
                $"OCR language pack not installed for {language.DisplayName} ({language.OcrLanguageTag}). " +
                "Install it in Windows Settings (Time & language → Language & region).");

        _language = language;
        _ocrEngine = engine;
    }

    private static OcrEngine? TryCreateOcrEngine(string languageTag, out string resolvedTag)
    {
        resolvedTag = languageTag;

        var engine = OcrEngine.TryCreateFromLanguage(new Language(languageTag));
        if (engine != null)
            return engine;

        var dash = languageTag.IndexOf('-');
        if (dash > 0)
        {
            resolvedTag = languageTag[..dash];
            engine = OcrEngine.TryCreateFromLanguage(new Language(resolvedTag));
            if (engine != null)
                return engine;
        }

        resolvedTag = "user-profile";
        return OcrEngine.TryCreateFromUserProfileLanguages();
    }

    public async Task<string> ExtractText(Bitmap bitmap)
    {
        Windows.Graphics.Imaging.SoftwareBitmap? softwareBitmap = null;
        try
        {
            softwareBitmap = await ScreenCapture.ConvertToSoftwareBitmap(bitmap);
            var result = await _ocrEngine.RecognizeAsync(softwareBitmap);
            return result.Text;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OCR Error: {ex.Message}");
            return string.Empty;
        }
        finally
        {
            softwareBitmap?.Dispose();
        }
    }

    public GameState DetectBannerState(string text) => DetectBannerState(text, _language);

    public static GameState DetectBannerState(string text, GameLanguage language)
    {
        if (string.IsNullOrWhiteSpace(text))
            return GameState.Idle;

        string normalized = OcrTextNormalizer.NormalizeForDetection(text);

        if (ContainsAny(normalized, language.GameFoundTokens))
            return GameState.GameFound;

        if (IsSearchingBanner(normalized, language))
            return GameState.Searching;

        // Accept screen shows CANCEL; the searching banner does not.
        if (ContainsAny(normalized, language.CancelTokens))
            return GameState.GameFound;

        return GameState.Idle;
    }

    public GameState DetectPreGameState(string text) => DetectPreGameState(text, _language);

    public static GameState DetectPreGameState(string text, GameLanguage language)
    {
        if (string.IsNullOrWhiteSpace(text))
            return GameState.Idle;

        string normalized = OcrTextNormalizer.NormalizeForDetection(text);

        if (ContainsAny(normalized, language.MatchStartingTokens))
            return GameState.MatchStarting;

        return GameState.Idle;
    }

    private static bool IsSearchingBanner(string normalized, GameLanguage language) =>
        ContainsAny(normalized, language.SearchingTokens);

    private static bool ContainsAny(string text, string[] tokens)
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
        var state = DetectBannerState(bannerText);

        if (state != GameState.Idle)
            return state;

        using var preGameCapture = ScreenCapture.CapturePreGameScreen();
        string preGameText = await ExtractText(preGameCapture);

        return DetectPreGameState(preGameText);
    }
}
