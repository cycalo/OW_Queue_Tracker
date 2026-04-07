using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace OWTrackerDesktop.Services;

/// <summary>
/// Picks a LAN IPv4 for the phone / QR code. Prefer real Wi‑Fi/Ethernet; deprioritize VPN, tunnel, and VM adapters.
/// </summary>
public static class NetworkAddressHelper
{
    public sealed record LanIpv4Choice(string Address, string Caption, int Score)
    {
        public override string ToString() => Caption;
    }

    /// <summary>All usable IPv4 candidates, best first (for dropdown).</summary>
    public static IReadOnlyList<LanIpv4Choice> GetRankedLanIpv4Choices()
    {
        var bestPerIp = new Dictionary<string, LanIpv4Choice>(StringComparer.Ordinal);

        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != OperationalStatus.Up)
                continue;
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            int typeBonus = GetInterfaceBonus(ni);
            string adapterName = GetAdapterDisplayName(ni);

            foreach (var ua in ni.GetIPProperties().UnicastAddresses)
            {
                var ip = ua.Address;
                if (ip.AddressFamily != AddressFamily.InterNetwork)
                    continue;
                if (IPAddress.IsLoopback(ip))
                    continue;

                int baseScore = ScoreLanAddressBase(ip);
                if (baseScore < 0)
                    continue;

                int total = baseScore + typeBonus + VirtualIpPenalty(ip);
                string addr = ip.ToString();

                if (!bestPerIp.TryGetValue(addr, out var existing) || total > existing.Score)
                    bestPerIp[addr] = new LanIpv4Choice(addr, adapterName, total);
            }
        }

        var ordered = bestPerIp.Values
            .OrderByDescending(c => c.Score)
            .ThenBy(c => c.Address, StringComparer.Ordinal)
            .ToList();

        var ambiguousCaptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var g in ordered.GroupBy(c => c.Caption, StringComparer.OrdinalIgnoreCase))
        {
            if (g.Count() > 1)
                ambiguousCaptions.Add(g.Key);
        }

        return ordered
            .Select(c => ambiguousCaptions.Contains(c.Caption)
                ? new LanIpv4Choice(c.Address, $"{c.Caption} ({c.Address})", c.Score)
                : c)
            .ToList();
    }

    public static string GetPreferredLanAdvertisedIPv4()
    {
        var list = GetRankedLanIpv4Choices();
        return list.Count > 0 ? list[0].Address : "127.0.0.1";
    }

    /// <summary>Windows "friendly" adapter name (same idea as Settings → Network).</summary>
    private static string GetAdapterDisplayName(NetworkInterface ni)
    {
        var desc = (ni.Description ?? string.Empty).Trim();
        if (desc.Length == 0)
            desc = (ni.Name ?? string.Empty).Trim();
        if (desc.Length == 0)
            return GetInterfaceKindLabel(ni);

        return desc;
    }

    private static string GetInterfaceKindLabel(NetworkInterface ni)
    {
        return ni.NetworkInterfaceType switch
        {
            NetworkInterfaceType.Wireless80211 => "Wi\u2011Fi",
            NetworkInterfaceType.Wman => "Wi\u2011Fi",
            NetworkInterfaceType.Ethernet => "Ethernet",
            NetworkInterfaceType.GigabitEthernet => "Ethernet",
            NetworkInterfaceType.FastEthernetT => "Ethernet",
            NetworkInterfaceType.Tunnel => "VPN / tunnel",
            NetworkInterfaceType.Ppp => "PPP",
            _ => ni.NetworkInterfaceType.ToString()
        };
    }

    private static int GetInterfaceBonus(NetworkInterface ni)
    {
        int b = 0;
        var d = ni.Description ?? string.Empty;
        if (d.Contains("VirtualBox", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("VMware", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("Hyper-V Virtual Ethernet", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("Default Switch", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("Wintun", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("TAP-Windows", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("ZeroTier", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("NordLynx", StringComparison.OrdinalIgnoreCase) ||
            d.Contains("WireGuard", StringComparison.OrdinalIgnoreCase))
        {
            b -= 950;
        }

        b += ni.NetworkInterfaceType switch
        {
            NetworkInterfaceType.Wireless80211 => 520,
            NetworkInterfaceType.Wman => 520,
            NetworkInterfaceType.Ethernet => 220,
            NetworkInterfaceType.GigabitEthernet => 220,
            NetworkInterfaceType.FastEthernetT => 220,
            NetworkInterfaceType.Tunnel => -1000,
            NetworkInterfaceType.Ppp => -280,
            _ => 0
        };

        return b;
    }

    private static int VirtualIpPenalty(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        if (bytes.Length != 4)
            return 0;

        // VirtualBox host-only
        if (bytes[0] == 192 && bytes[1] == 168 && bytes[2] == 56)
            return -650;
        // Windows mobile hotspot / ICS host range (often not the LAN path to other Wi‑Fi clients)
        if (bytes[0] == 192 && bytes[1] == 168 && bytes[2] == 137)
            return -550;
        if (bytes[0] == 169 && bytes[1] == 254)
            return -450;

        // Typical home router LAN — slight preference over 10.x (often VPN overlay)
        if (bytes[0] == 192 && bytes[1] == 168)
            return 50;
        return 0;
    }

    private static int ScoreLanAddressBase(IPAddress ip)
    {
        var bytes = ip.GetAddressBytes();
        if (bytes.Length != 4)
            return -1;

        if (bytes[0] == 192 && bytes[1] == 168)
            return 320;
        if (bytes[0] == 10)
            return 260;
        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
            return 270;

        if (bytes[0] == 169 && bytes[1] == 254)
            return 90;

        return 130;
    }
}
