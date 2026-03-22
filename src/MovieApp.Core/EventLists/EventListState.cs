namespace MovieApp.Core.EventLists;

/// <summary>
/// Current search, sort, and filter state for an event list screen.
/// </summary>
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
        EventSortOption.HistoricalRatingDescending,
    ];

    /// <summary>
    /// Creates a deep-enough copy for UI workflows that need to compare or edit state.
    /// </summary>
    public EventListState Clone()
    {
        return new EventListState
        {
            SearchText = SearchText,
            SelectedSortOption = SelectedSortOption,
            ActiveFilters = ActiveFilters.Clone(),
        };
    }

    /// <summary>
    /// Restores the search, sort, and filter state to the screen defaults.
    /// </summary>
    public void Reset()
    {
        SearchText = string.Empty;
        SelectedSortOption = EventSortOption.DateAscending;
        ActiveFilters.Reset();
    }
}
