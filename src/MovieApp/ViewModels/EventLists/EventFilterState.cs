namespace MovieApp.ViewModels.EventLists;

public sealed class EventFilterState
{
    public string? EventType { get; set; }

    public string? LocationReference { get; set; }

    public decimal? MinimumTicketPrice { get; set; }

    public decimal? MaximumTicketPrice { get; set; }

    public bool OnlyAvailableEvents { get; set; }

    public bool HasActiveFilters()
    {
        // TODO: Return true when at least one filter property is meaningfully set.
        throw new NotImplementedException();
    }

    public EventFilterState Clone()
    {
        // TODO: Return a defensive copy so each screen can manage its own filter state.
        throw new NotImplementedException();
    }
}
