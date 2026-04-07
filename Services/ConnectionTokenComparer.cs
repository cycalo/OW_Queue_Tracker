using System.Security.Cryptography;
using System.Text;

namespace OWTrackerDesktop.Services;

public static class ConnectionTokenComparer
{
    public static bool Matches(string? provided, string expected)
    {
        if (string.IsNullOrEmpty(provided) || string.IsNullOrEmpty(expected))
            return false;

        var p = Encoding.UTF8.GetBytes(provided.Trim());
        var e = Encoding.UTF8.GetBytes(expected.Trim());
        if (p.Length != e.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(p, e);
    }
}
