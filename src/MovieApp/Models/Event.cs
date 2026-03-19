namespace MovieApp.Models;

public sealed class Event
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; set; }
    public string PosterUrl { get; set; } = "";
    public required DateTime EventDateTime { get; init; }
    public required string LocationReference { get; set; }
    public required decimal TicketPrice { get; set; }

    
    public double HistoricalRating { get; set; } = 0.0;
    public required string EventType { get; set; }
    public int MaxCapacity { get; set; } = 50;
    public int CurrentEnrollment { get; set; } = 0;

    public required int CreatorUserId { get; init; }

    public int AvailableSpots => MaxCapacity - CurrentEnrollment;
}