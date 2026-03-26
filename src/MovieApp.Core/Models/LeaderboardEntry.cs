namespace MovieApp.Core.Models;

public sealed class LeaderboardEntry
{
    public required int UserId { get; init; }
    public required string Username { get; init; }
    public int CompletedMoviesCount { get; init; }
    public double TriviaAccuracy { get; init; }
    public DateTime? FinishedAt { get; init; }
}