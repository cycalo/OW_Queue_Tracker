using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

public class OWWebSocketServer
{
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

    public bool IsRunning { get; private set; }
    public string LocalIP { get; private set; } = "127.0.0.1";
    public int Port => _port;

    public OWWebSocketServer(int port = 8080)
    {
        _port = port;
    }

    public void Start()
    {
        try
        {
            LocalIP = GetLocalIPAddress();
            _server = new WebSocketSharp.Server.WebSocketServer($"ws://{LocalIP}:{_port}");
            _server.AddWebSocketService<OWTrackerBehavior>("/");
            _server.Start();
            IsRunning = true;
            System.Diagnostics.Debug.WriteLine($"WebSocket server started at ws://{LocalIP}:{_port}");
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

    private static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "127.0.0.1";
    }
}

public class OWTrackerBehavior : WebSocketBehavior
{
    protected override void OnOpen()
    {
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

    protected override void OnClose(CloseEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Client disconnected: {ID}");
        OWWebSocketServer.OnConnectionCountChanged?.Invoke();
        // Session may be removed from collection after OnClose; refresh again after a short delay so count is correct
        System.Threading.Tasks.Task.Run(async () =>
        {
            await System.Threading.Tasks.Task.Delay(100);
            OWWebSocketServer.OnConnectionCountChanged?.Invoke();
        });
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Received from client: {e.Data}");
    }

    protected override void OnError(WebSocketSharp.ErrorEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"WebSocket error: {e.Message}");
    }
}
