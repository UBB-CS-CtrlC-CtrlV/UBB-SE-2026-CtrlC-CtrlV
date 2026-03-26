namespace MovieApp.Core.Models;

public sealed class MarathonDisplayItem
{
    public required Marathon Marathon { get; init; }
    public int ParticipantCount { get; init; }
    public double UserAccuracy { get; init; }
    public bool IsJoinedByUser { get; init; }
    public int UserMoviesVerified { get; init; }
    public int TotalMovies { get; init; }
    public DateTime WeekEnd { get; init; }

    public Event ToEvent()
    {
        var location = Marathon.PrerequisiteMarathonId.HasValue
            ? "⚡ Elite Marathon"
            : "Standard Marathon";

        var rating = IsJoinedByUser ? UserAccuracy / 20.0 : 0.0;
        rating = Math.Round(Math.Min(rating, 5.0), 1);

        return new Event
        {
            Id = Marathon.Id,
            Title = Marathon.Title,
            Description = Marathon.Description ?? "A weekly themed movie marathon.",
            EventType = Marathon.Theme ?? "Marathon",
            EventDateTime = WeekEnd,
            LocationReference = location,
            TicketPrice = 0,
            HistoricalRating = rating,
            MaxCapacity = 999999,
            CurrentEnrollment = ParticipantCount,
            CreatorUserId = 0,
        };
    }
}