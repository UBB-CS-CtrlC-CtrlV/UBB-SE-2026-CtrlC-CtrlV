namespace MovieApp.Core.Models;

public sealed class WatchedEvent
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public decimal TargetPrice { get; set; }
}