using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

public class OWWebSocketServer
{
    public const int MaxWebSocketIncomingBytes = 4096;
    public const int MaxWebSocketConnections = 8;

    private WebSocketSharp.Server.WebSocketServer? _server;
    private readonly int _port;

    /// <summary>
    /// Called when a client connects so we can send the current game state right after the welcome message.
    /// Set by the app (e.g. MainForm) to return the current state from GameMonitor.
    /// </summary>
    public static Func<GameStateEvent?>? GetCurrentStateOnConnect { get; set; }

    /// <summary>
    /// Invoked when a client connects or disconnects so the UI can refresh the connection count.
    /// Set by the app (e.g. MainForm) to call UpdateStatus on the UI thread.
    /// </summary>
    public static Action? OnConnectionCountChanged { get; set; }

    /// <summary>
    /// Shared secret; clients must pass the same value as query <c>token</c> (or <c>auth</c>).
    /// </summary>
    public static string ConnectionToken { get; private set; } = "";

    /// <summary>Clears static hooks and token after shutdown (optional hygiene).</summary>
    public static void ReleaseStaticState()
    {
        GetCurrentStateOnConnect = null;
        OnConnectionCountChanged = null;
        ConnectionToken = "";
    }

    public bool IsRunning { get; private set; }
    /// <summary>IPv4 to show in the UI for the phone (not necessarily the bind address).</summary>
    public string AdvertisedLanIP { get; private set; } = "127.0.0.1";

    /// <summary>Alias for <see cref="AdvertisedLanIP"/> (LAN address shown to the user).</summary>
    public string LocalIP => AdvertisedLanIP;

    public int Port => _port;

    /// <summary>
    /// Full WebSocket URI for the phone (QR code, clipboard). Empty if not running or no token.
    /// </summary>
    public string GetConnectionWebSocketUri()
    {
        if (!IsRunning || string.IsNullOrEmpty(ConnectionToken))
            return string.Empty;

        var token = Uri.EscapeDataString(ConnectionToken);
        return $"ws://{AdvertisedLanIP}:{_port}/?token={token}";
    }

    public OWWebSocketServer(int port = 8080)
    {
        _port = port;
    }

    /// <summary>
    /// Updates the IP embedded in the QR code / shown to the user. Listener stays on all interfaces.
    /// </summary>
    public bool TrySetAdvertisedLanIp(string? ipv4)
    {
        if (string.IsNullOrWhiteSpace(ipv4))
            return false;
        if (!IPAddress.TryParse(ipv4.Trim(), out var ip))
            return false;
        if (ip.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(ip))
            return false;

        AdvertisedLanIP = ip.ToString();
        return true;
    }

    public void Start()
    {
        try
        {
            ConnectionToken = ConnectionSecretStore.LoadOrCreate();
            AdvertisedLanIP = NetworkAddressHelper.GetPreferredLanAdvertisedIPv4();

            // Listen on all IPv4 interfaces so the phone can use any correct LAN IP for this PC.
            _server = new WebSocketSharp.Server.WebSocketServer(IPAddress.Any, _port);
            _server.WaitTime = TimeSpan.FromSeconds(30);
            _server.KeepClean = true;
            _server.Log.Level = LogLevel.Fatal;

            _server.AddWebSocketService<OWTrackerBehavior>("/");
            _server.Start();
            IsRunning = true;
            System.Diagnostics.Debug.WriteLine(
                $"WebSocket server listening on 0.0.0.0:{_port}; advertise ws://{AdvertisedLanIP}:{_port}/?token=***");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start WebSocket server: {ex.Message}");
            IsRunning = false;
        }
    }

    public void Stop()
    {
        if (_server is not null && IsRunning)
        {
            _server.Stop();
            IsRunning = false;
        }
    }

    public void BroadcastGameState(GameStateEvent evt)
    {
        if (!IsRunning || _server is null) return;

        string json = SerializeStateMessage(evt);
        _server.WebSocketServices["/"].Sessions.Broadcast(json);
        System.Diagnostics.Debug.WriteLine(
            $"Broadcast: {evt.State} to {GetConnectedClientCount()} client(s)");
    }

    /// <summary>
    /// Same JSON shape as state broadcasts. Used for broadcast and for sending current state on connect.
    /// </summary>
    public static string SerializeStateMessage(GameStateEvent evt)
    {
        var payload = new
        {
            type = evt.State.ToString().ToLowerInvariant(),
            data = new
            {
                state = evt.State.ToString(),
                message = evt.Message
            },
            timestamp = evt.Timestamp.ToString("o")
        };
        return JsonConvert.SerializeObject(payload);
    }

    public int GetConnectedClientCount()
    {
        if (!IsRunning || _server is null) return 0;
        return _server.WebSocketServices["/"].Sessions.Count;
    }
}

public class OWTrackerBehavior : WebSocketBehavior
{
    protected override void OnOpen()
    {
        if (!TryAuthorize(out var denyReason))
        {
            Context.WebSocket.Close(CloseStatusCode.PolicyViolation, denyReason);
            return;
        }

        System.Diagnostics.Debug.WriteLine($"Client connected: {ID}");

        var welcome = new
        {
            type = "connected",
            data = new { message = "Connected to Overwatch Queue Tracker" },
            timestamp = DateTime.UtcNow.ToString("o")
        };
        Send(JsonConvert.SerializeObject(welcome));

        var currentState = OWWebSocketServer.GetCurrentStateOnConnect?.Invoke();
        if (currentState != null)
        {
            Send(OWWebSocketServer.SerializeStateMessage(currentState));
        }

        OWWebSocketServer.OnConnectionCountChanged?.Invoke();
    }

    private bool TryAuthorize(out string denyReason)
    {
        denyReason = "";

        // New session is typically not counted yet; reject when we already have Max clients.
        if (Sessions.Count >= OWWebSocketServer.MaxWebSocketConnections)
        {
            denyReason = "Too many connections";
            return false;
        }

        var qs = Context.QueryString;
        var token = qs["token"] ?? qs["auth"];
        if (!ConnectionTokenComparer.Matches(token, OWWebSocketServer.ConnectionToken))
        {
            denyReason = "Invalid or missing token";
            return false;
        }

        return true;
    }

    protected override void OnClose(CloseEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Client disconnected: {ID}");
        OWWebSocketServer.OnConnectionCountChanged?.Invoke();
        System.Threading.Tasks.Task.Run(async () =>
        {
            await System.Threading.Tasks.Task.Delay(100);
            OWWebSocketServer.OnConnectionCountChanged?.Invoke();
        });
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        if (e.IsPing)
            return;

        int len = e.IsText ? (e.Data?.Length ?? 0) : e.RawData.Length;
        if (len > OWWebSocketServer.MaxWebSocketIncomingBytes)
        {
            System.Diagnostics.Debug.WriteLine($"Rejecting oversized message ({len} bytes) from {ID}");
            Context.WebSocket.Close(CloseStatusCode.TooBig, "Message too large");
            return;
        }

#if DEBUG
        var previewLen = Math.Min(64, len);
        var preview = e.IsText && e.Data != null && previewLen > 0
            ? e.Data.Substring(0, previewLen)
            : $"<binary {len} B>";
        System.Diagnostics.Debug.WriteLine($"Received from client {ID}: len={len} preview={preview}");
#endif
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"WebSocket error: {e.Message}");
    }
}
