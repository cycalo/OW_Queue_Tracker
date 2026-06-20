using OWTrackerDesktop.Models;

namespace OWTrackerDesktop.Services;

public class GameMonitor
{
    private readonly OCRService _ocrService;
    private readonly OWWebSocketServer _webSocketServer;
    private CancellationTokenSource? _cts;
    private GameState _lastState = GameState.Idle;
    private readonly int _pollIntervalMs;
    private const int ActiveQueuePollIntervalMs = 750;
    private int _consecutiveErrors;

    public bool IsMonitoring { get; private set; }

    public event Action<GameState, GameState>? StateChanged;

    /// <summary>Fired after several consecutive OCR/capture failures (monitor still running).</summary>
    public event Action<string>? PersistentFailure;

    public GameState CurrentState => _lastState;

    public GameMonitor(OWWebSocketServer webSocketServer, int pollIntervalMs = 2000)
    {
        _ocrService = new OCRService(GameLanguageStore.LoadOrDefault());
        _webSocketServer = webSocketServer;
        _pollIntervalMs = pollIntervalMs;
    }

    public void SetGameLanguage(GameLanguage language)
    {
        GameLanguageStore.Save(language.Id);
        _ocrService.SetLanguage(language);
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
                _consecutiveErrors = 0;

                if (currentState != _lastState)
                {
                    var previous = _lastState;
                    _lastState = currentState;

                    var evt = new GameStateEvent(currentState);
                    _webSocketServer.BroadcastGameState(evt);
                    StateChanged?.Invoke(previous, currentState);

                    System.Diagnostics.Debug.WriteLine(
                        $"State changed: {previous} -> {currentState}");
                }

                var pollMs = _lastState is GameState.Searching or GameState.GameFound
                    ? ActiveQueuePollIntervalMs
                    : _pollIntervalMs;
                await Task.Delay(pollMs, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _consecutiveErrors++;
                System.Diagnostics.Debug.WriteLine($"Monitor error: {ex.Message}");
                if (_consecutiveErrors == 5 || (_consecutiveErrors > 5 && _consecutiveErrors % 10 == 0))
                {
                    var msg = ex.Message.Length > 200 ? ex.Message[..200] + "…" : ex.Message;
                    PersistentFailure?.Invoke(
                        $"Screen/OCR failed {_consecutiveErrors} times in a row. Last error: {msg}");
                }

                await Task.Delay(5000, ct);
            }
        }
    }
}
