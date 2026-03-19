using MovieApp.Core.EventLists;
using MovieApp.Core.Models;

namespace MovieApp.ViewModels.Events;

public abstract class EventListPageViewModel : ViewModelBase
{
    private IReadOnlyList<Event> _allEvents = [];
    private IReadOnlyList<Event> _visibleEvents = [];

    protected EventListPageViewModel()
    {
        ListState = new EventListState();
    }

    public abstract string PageTitle { get; }

    public EventListState ListState { get; }

    public IReadOnlyList<EventSortOption> AvailableSortOptions => ListState.AvailableSortOptions;

    public IReadOnlyList<Event> AllEvents
    {
        get => _allEvents;
        protected set => SetProperty(ref _allEvents, value);
    }

    public IReadOnlyList<Event> VisibleEvents
    {
        get => _visibleEvents;
        protected set => SetProperty(ref _visibleEvents, value);
    }

    public Task InitializeAsync()
    {
        // TODO: Load this screen's events, assign AllEvents, and call RefreshVisibleEvents.
        throw new NotImplementedException();
    }

    public void SetSearchText(string? searchText)
    {
        // TODO: Copy the incoming value into ListState.SearchText and refresh VisibleEvents.
        throw new NotImplementedException();
    }

    public void SetSortOption(EventSortOption sortOption)
    {
        // TODO: Update ListState.SelectedSortOption and refresh VisibleEvents.
        throw new NotImplementedException();
    }

    public void UpdateFilters(Action<EventFilterState> updateFilters)
    {
        // TODO: Apply the provided filter mutation to ListState.ActiveFilters and refresh VisibleEvents.
        throw new NotImplementedException();
    }

    public void ResetListState()
    {
        // TODO: Reset the per-screen state and refresh VisibleEvents.
        throw new NotImplementedException();
    }

    public void RefreshVisibleEvents()
    {
        // TODO: Rebuild VisibleEvents with EventListTransformer.Apply(AllEvents, ListState).
        throw new NotImplementedException();
    }

    protected abstract Task<IReadOnlyList<Event>> LoadEventsAsync();

    protected virtual IReadOnlyList<Event> BuildSampleEvents()
    {
        // TODO: Return sample events for this screen while the real data source is not wired up yet.
        throw new NotImplementedException();
    }
}
