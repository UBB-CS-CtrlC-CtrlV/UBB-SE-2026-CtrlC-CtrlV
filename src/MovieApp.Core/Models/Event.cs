namespace MovieApp.Core.Models;

public sealed class Event
{
    public required int Id { get; init; }

    public required string Title { get; init; }

    public string? Description { get; set; }

    public string PosterUrl { get; set; } = string.Empty;

    public required DateTime EventDateTime { get; init; }

    public required string LocationReference { get; set; }

    public required decimal TicketPrice { get; set; }

    public double HistoricalRating { get; set; }

    public int MaxCapacity { get; set; } = 50;

    public int CurrentEnrollment { get; set; }

    public string EventType { get; set; } = string.Empty;

    public required int CreatorUserId { get; init; }

    public int AvailableSpots => MaxCapacity - CurrentEnrollment;

    public bool IsAvailable => AvailableSpots > 0 && EventDateTime > DateTime.Now;
}
