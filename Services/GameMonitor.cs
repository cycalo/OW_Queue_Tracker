using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

public class GameMonitor
{
    private readonly OCRService _ocrService;
    private readonly OWWebSocketServer _webSocketServer;
    private CancellationTokenSource? _cts;
    private GameState _lastState = GameState.Idle;
    private readonly int _pollIntervalMs;

    public bool IsMonitoring { get; private set; }

    public event Action<GameState, GameState>? StateChanged;

    public GameState CurrentState => _lastState;

    public GameMonitor(OWWebSocketServer webSocketServer, int pollIntervalMs = 2000)
    {
        _ocrService = new OCRService();
        _webSocketServer = webSocketServer;
        _pollIntervalMs = pollIntervalMs;
    }

    public void Start()
    {
        if (IsMonitoring) return;

        _cts = new CancellationTokenSource();
        IsMonitoring = true;
        _ = MonitorLoop(_cts.Token);
        System.Diagnostics.Debug.WriteLine("Game monitoring started");
    }

    public void Stop()
    {
        if (!IsMonitoring) return;

        _cts?.Cancel();
        IsMonitoring = false;
        System.Diagnostics.Debug.WriteLine("Game monitoring stopped");
    }

    private async Task MonitorLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var currentState = await _ocrService.DetectCurrentState();

                if (currentState != _lastState)
                {
                    var previous = _lastState;
                    _lastState = currentState;

                    var evt = new GameStateEvent(currentState);
                    _webSocketServer.BroadcastGameState(evt);
                    StateChanged?.Invoke(previous, currentState);

                    Console.WriteLine($"[Monitor] State changed: {previous} -> {currentState}");
                    System.Diagnostics.Debug.WriteLine(
                        $"State changed: {previous} -> {currentState}");
                }

                await Task.Delay(_pollIntervalMs, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Monitor error: {ex.Message}");
                await Task.Delay(5000, ct);
            }
        }
    }
}
