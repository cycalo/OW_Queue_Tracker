namespace OWTrackerDesktop.Services;

public static class OcrTextNormalizer
{
    public static string NormalizeForDetection(string text)
    {
        var chars = new char[text.Length];
        int index = 0;

        foreach (char ch in text)
        {
            if (char.IsLetterOrDigit(ch))
                chars[index++] = char.ToLowerInvariant(ch);
        }

        return new string(chars, 0, index);
    }

    public static string[] NormalizeTokens(params string[] tokens) =>
        tokens.Select(NormalizeForDetection).Where(t => t.Length > 0).Distinct().ToArray();
}
