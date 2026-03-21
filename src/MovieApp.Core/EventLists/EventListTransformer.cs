using MovieApp.Core.Models;

namespace MovieApp.Core.EventLists;

public static class EventListTransformer
{
    public static IReadOnlyList<Event> Apply(IEnumerable<Event> events, EventListState state)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(state);

        var filteredEvents = ApplyFilters(events, state.ActiveFilters);
        var searchedEvents = ApplySearch(filteredEvents, state.SearchText);
        var sortedEvents = ApplySorting(searchedEvents, state.SelectedSortOption);

        return sortedEvents.ToList();
    }

    public static IEnumerable<Event> ApplyFilters(IEnumerable<Event> events, EventFilterState filters)
    {
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(filters);

        if (filters.MinimumTicketPrice.HasValue
            && filters.MaximumTicketPrice.HasValue
            && filters.MinimumTicketPrice.Value > filters.MaximumTicketPrice.Value)
        {
            return [];
        }

        var eventType = string.IsNullOrWhiteSpace(filters.EventType) ? null : filters.EventType.Trim();
        var locationReference = string.IsNullOrWhiteSpace(filters.LocationReference)
            ? null
            : filters.LocationReference.Trim();

        return events.Where(e =>
            (!filters.OnlyAvailableEvents || e.IsAvailable)
            && (locationReference is null
                || string.Equals(e.LocationReference, locationReference, StringComparison.OrdinalIgnoreCase))
            && (!filters.MinimumTicketPrice.HasValue || e.TicketPrice >= filters.MinimumTicketPrice.Value)
            && (!filters.MaximumTicketPrice.HasValue || e.TicketPrice <= filters.MaximumTicketPrice.Value));
    }

    public static IEnumerable<Event> ApplySearch(IEnumerable<Event> events, string? searchText)
    {
        ArgumentNullException.ThrowIfNull(events);

        if (string.IsNullOrWhiteSpace(searchText))
        {
            return events;
        }

        var query = searchText.Trim();

        return events.Where(e =>
            e.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (e.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || e.LocationReference.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public static IOrderedEnumerable<Event> ApplySorting(IEnumerable<Event> events, EventSortOption sortOption)
    {
        ArgumentNullException.ThrowIfNull(events);

        return sortOption switch
        {
            EventSortOption.DateAscending => events.OrderBy(e => e.EventDateTime).ThenBy(e => e.Id),
            EventSortOption.DateDescending => events.OrderByDescending(e => e.EventDateTime).ThenBy(e => e.Id),
            EventSortOption.PriceAscending => events.OrderBy(e => e.TicketPrice).ThenBy(e => e.Id),
            EventSortOption.PriceDescending => events.OrderByDescending(e => e.TicketPrice).ThenBy(e => e.Id),
            EventSortOption.HistoricalRatingDescending => events.OrderByDescending(e => e.HistoricalRating).ThenBy(e => e.Id),
            _ => throw new ArgumentOutOfRangeException(nameof(sortOption), sortOption, null),
        };
    }
}
