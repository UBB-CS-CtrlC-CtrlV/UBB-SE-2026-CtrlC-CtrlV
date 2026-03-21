namespace MovieApp.Core.Models;

public sealed class MarathonProgress
{
    public required int UserId { get; init; }
    public required int MarathonId { get; init; }

    public DateTime JoinedAt { get; set; } = DateTime.Now;

    public double TriviaAccuracy { get; set; }

    public int CompletedMoviesCount { get; set; }

    public DateTime? FinishedAt { get; set; }

    public bool IsCompleted => FinishedAt.HasValue;
}