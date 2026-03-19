using MovieApp.Core.EventLists;
using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Base view model for screens that display a searchable, filterable,
/// and sortable event list.
/// </summary>
/// <remarks>
/// <see cref="AllEvents"/> stores the source data for the screen.
/// <br/>
/// <see cref="VisibleEvents"/> stores the transformed list shown in the UI.
/// <br/>
/// Call <see cref="RefreshVisibleEvents"/> after changing <see cref="EventListState"/>
/// or replacing <see cref="AllEvents"/>.
/// </remarks>
public abstract class EventListPageViewModel : ViewModelBase
{
    private IReadOnlyList<Event> _allEvents = [];
    private IReadOnlyList<Event> _visibleEvents = [];

    public abstract string PageTitle { get; }

    public EventListState EventListState { get; } = new();

    public IReadOnlyList<EventSortOption> AvailableSortOptions => EventListState.AvailableSortOptions;

    /// <summary>
    /// The full source event list owned by this screen.
    /// </summary>
    public IReadOnlyList<Event> AllEvents
    {
        get => _allEvents;
        protected set => SetProperty(ref _allEvents, value);
    }

    /// <summary>
    /// The transformed event list currently displayed by the UI.
    /// </summary>
    public IReadOnlyList<Event> VisibleEvents
    {
        get => _visibleEvents;
        protected set => SetProperty(ref _visibleEvents, value);
    }

    public Task InitializeAsync()
    {
        // TODO: 6 Load this screen's events, assign AllEvents, and call RefreshVisibleEvents.
        throw new NotImplementedException();
    }

    /// <summary>
    /// Updates <see cref="EventListState.SearchText"/> and refreshes
    /// <see cref="VisibleEvents"/>.
    /// </summary>
    /// <param name="searchText">
    /// The new search text. A <see langword="null"/> value is treated as an empty string.
    /// </param>
    /// <remarks>
    /// If the effective value of <see cref="searchText"/> is not different from
    /// <see cref="EventListState.SearchText"/> the refresh will not be performed.
    /// </remarks>
    public void SetSearchText(string? searchText)
    {
        // Passing in a null value must allow the user to reset search.
        var normalizedSearchText = searchText ?? string.Empty;
        if (EventListState.SearchText == normalizedSearchText)
        {
            // Refresh not needed if the search text didn't change
            return;
        }
        EventListState.SearchText = normalizedSearchText;
        RefreshVisibleEvents();
    }

    /// <summary>
    /// Updates <see cref="EventListState.SelectedSortOption"/> and refreshes
    /// <see cref="VisibleEvents"/>.
    /// </summary>
    /// <param name="sortOption">
    /// The new sort option.
    /// </param>
    /// <remarks>
    /// If the effective value of <see cref="sortOption"/> is not different from
    /// <see cref="EventListState.SelectedSortOption"/> the refresh will not be performed.
    /// </remarks>
    public void SetSortOption(EventSortOption sortOption)
    {
        if (EventListState.SelectedSortOption == sortOption)
        {
            // Refresh not needed if the sort option did not change.
            return;
        }
        EventListState.SelectedSortOption = sortOption;
        RefreshVisibleEvents();
    }

    /// <summary>
    /// Applies a caller-provided update to <see cref="EventListState.ActiveFilters"/>
    /// and refreshes <see cref="VisibleEvents"/>.
    /// </summary>
    /// <param name="updateFilters">
    /// A delegate that mutates the current filter state for the current screen.
    /// </param>
    /// <remarks>
    /// Use this method to keep all filter changes centralized so the derived
    /// event list is always recomputed after filter state changes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="updateFilters"/> is <see langword="null"/>.
    /// </exception>
    public void UpdateFilters(Action<EventFilterState> updateFilters)
    {
        ArgumentNullException.ThrowIfNull(updateFilters);
        
        updateFilters(EventListState.ActiveFilters);
        RefreshVisibleEvents();
    }

    /// <summary>
    /// Resets the <see cref="EventListState"/> to its default values and refreshes
    /// <see cref="VisibleEvents"/>.
    /// </summary>
    /// <remarks>
    /// This clears the current search text, selected sort option and active filters
    /// for the current screen.
    /// </remarks>
    public void ResetEventListState()
    {
        EventListState.Reset();
        RefreshVisibleEvents();
    }

    /// <summary>
    /// Rebuilds the visible event list by applying the current search, filter,
    /// and sort state to <see cref="AllEvents"/>.
    /// </summary>
    /// <remarks>
    /// This method must be called after any change to <see cref="EventListState"/>
    /// so that <see cref="VisibleEvents"/> raises a property change notification
    /// and the UI can refresh.
    /// </remarks>
    public void RefreshVisibleEvents()
    {
        VisibleEvents = EventListTransformer.Apply(AllEvents, EventListState);
    }

    protected abstract Task<IReadOnlyList<Event>> LoadEventsAsync();
}
