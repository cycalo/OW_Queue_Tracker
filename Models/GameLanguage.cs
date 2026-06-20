namespace OWTrackerDesktop.Models;

public sealed class GameLanguage
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string OcrLanguageTag { get; init; }
    public required string[] GameFoundTokens { get; init; }
    public required string[] SearchingTokens { get; init; }
    public required string[] CancelTokens { get; init; }
    public required string[] MatchStartingTokens { get; init; }
}
