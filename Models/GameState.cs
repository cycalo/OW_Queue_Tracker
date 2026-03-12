namespace OWTrackerDesktop.Models;

public enum GameState
{
    Idle,
    Searching,
    GameFound,
    MatchStarting
}

public class GameStateEvent
{
    public GameState State { get; set; }
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }

    public GameStateEvent(GameState state, string message = "")
    {
        State = state;
        Timestamp = DateTime.UtcNow;
        Message = string.IsNullOrEmpty(message) ? GetStateMessage(state) : message;
    }

    public static string GetStateMessage(GameState state) => state switch
    {
        GameState.Searching => "Searching for game...",
        GameState.GameFound => "Game found! Get ready!",
        GameState.MatchStarting => "Match starting soon",
        GameState.Idle => "Ready to queue",
        _ => ""
    };
}
