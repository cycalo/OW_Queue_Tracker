using System.Security.Cryptography;
using System.Text;

namespace OWTrackerDesktop.Services;

/// <summary>
/// Persists a LAN connection secret under LocalApplicationData (not in the repo).
/// </summary>
public static class ConnectionSecretStore
{
    private static readonly string SecretFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "OWTrackerDesktop",
        "connection.secret");

    /// <summary>
    /// Loads the existing secret or creates a new 128-bit hex token (32 chars).
    /// </summary>
    public static string LoadOrCreate()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SecretFilePath)!);

            if (File.Exists(SecretFilePath))
            {
                var existing = File.ReadAllText(SecretFilePath).Trim();
                if (IsValidStoredToken(existing))
                    return existing;
            }

            var fresh = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
            var bytes = Encoding.UTF8.GetBytes(fresh);
            File.WriteAllBytes(SecretFilePath, bytes);
            return fresh;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ConnectionSecretStore: {ex.Message}");
            // Fallback: ephemeral secret for this process only (phone cannot connect until disk works).
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        }
    }

    private static bool IsValidStoredToken(string s)
    {
        if (s.Length != 32)
            return false;
        foreach (var c in s)
        {
            if (c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F')
                continue;
            return false;
        }

        return true;
    }
}
