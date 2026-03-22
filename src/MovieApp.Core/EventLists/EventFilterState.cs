namespace MovieApp.Core.EventLists;

/// <summary>
/// Mutable filter criteria applied to an event list screen.
/// </summary>
public sealed class EventFilterState
{
    public string? EventType { get; set; }

    public string? LocationReference { get; set; }

    public decimal? MinimumTicketPrice { get; set; }

    public decimal? MaximumTicketPrice { get; set; }

    public bool OnlyAvailableEvents { get; set; }

    /// <summary>
    /// Indicates whether any filter currently changes the visible event list.
    /// </summary>
    public bool HasActiveFilters()
    {
        return !string.IsNullOrWhiteSpace(EventType)
            || !string.IsNullOrWhiteSpace(LocationReference)
            || MinimumTicketPrice is not null
            || MaximumTicketPrice is not null
            || OnlyAvailableEvents;
    }

    /// <summary>
    /// Creates a copy so callers can snapshot or branch filter state safely.
    /// </summary>
    public EventFilterState Clone()
    {
        return new EventFilterState
        {
            EventType = EventType,
            LocationReference = LocationReference,
            MinimumTicketPrice = MinimumTicketPrice,
            MaximumTicketPrice = MaximumTicketPrice,
            OnlyAvailableEvents = OnlyAvailableEvents,
        };
    }

    /// <summary>
    /// Restores all filters to their default unset values.
    /// </summary>
    public void Reset()
    {
        EventType = null;
        LocationReference = null;
        MinimumTicketPrice = null;
        MaximumTicketPrice = null;
        OnlyAvailableEvents = false;
    }
}
