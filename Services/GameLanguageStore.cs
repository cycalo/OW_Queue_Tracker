using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

/// <summary>
/// Persists the selected Overwatch game language under LocalApplicationData.
/// </summary>
public static class GameLanguageStore
{
    private static readonly string LanguageFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OWTrackerDesktop",
        "game.language");

    public static GameLanguage LoadOrDefault()
    {
        try
        {
            if (File.Exists(LanguageFilePath))
            {
                var id = File.ReadAllText(LanguageFilePath).Trim();
                if (id.Equals("en-us", StringComparison.OrdinalIgnoreCase) ||
                    id.Equals("en-gb", StringComparison.OrdinalIgnoreCase))
                    id = "en";

                var language = GameLanguageCatalog.FindById(id);
                if (language != null)
                    return language;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameLanguageStore: {ex.Message}");
        }

        return GameLanguageCatalog.Default;
    }

    public static void Save(string languageId)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LanguageFilePath)!);
            File.WriteAllText(LanguageFilePath, languageId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GameLanguageStore: {ex.Message}");
        }
    }
}
