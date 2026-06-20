namespace OWTrackerDesktop.Services;

/// <summary>
/// Desktop UI strings for the selected app language (same id as <see cref="GameLanguageCatalog"/>).
/// </summary>
public static class AppLocalizer
{
    private static string _currentId = "en";

    public static string CurrentLanguageId => _currentId;

    public static void SetLanguage(string languageId)
    {
        if (!UiStringTable.Contains(languageId))
            languageId = "en";

        _currentId = languageId;
    }

    public static string T(string key) => UiStringTable.Get(_currentId, key);

    public static string T(string key, params object[] args) =>
        string.Format(T(key), args);
}
