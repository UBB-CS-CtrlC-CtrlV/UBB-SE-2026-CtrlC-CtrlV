namespace MovieApp.ViewModels.EventLists;

public sealed class EventListState
{
    public string SearchText { get; set; } = string.Empty;

    public EventSortOption SelectedSortOption { get; set; } = EventSortOption.DateAscending;

    public EventFilterState ActiveFilters { get; set; } = new();

    public IReadOnlyList<EventSortOption> AvailableSortOptions { get; } =
    [
        EventSortOption.DateAscending,
        EventSortOption.DateDescending,
        EventSortOption.PriceAscending,
        EventSortOption.PriceDescending,
        EventSortOption.RatingDescending,
        EventSortOption.TitleAscending,
    ];

    public EventListState Clone()
    {
        // TODO: Return a deep-enough copy so view state can stay isolated per screen.
        throw new NotImplementedException();
    }

    public void Reset()
    {
        // TODO: Reset search text, selected sort option, and active filters to defaults.
        throw new NotImplementedException();
    }
}
