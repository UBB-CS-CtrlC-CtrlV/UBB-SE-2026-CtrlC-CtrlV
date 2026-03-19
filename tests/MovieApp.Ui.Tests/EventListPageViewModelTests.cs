using System.ComponentModel;
using MovieApp.Core.EventLists;
using MovieApp.Core.Models;
using MovieApp.Ui.ViewModels.Events;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class EventListPageViewModelTests
{
    [Fact]
    public async Task InitializeAsync_LoadsAllEventsAndRefreshesVisibleEventsUsingCurrentState()
    {
        var viewModel = new TestEventListPageViewModel(BuildSampleEvents());
        viewModel.SetSearchText("festival");
        viewModel.SetSortOption(EventSortOption.PriceDescending);

        await viewModel.InitializeAsync();

        Assert.Equal([1, 2, 3, 4], viewModel.AllEvents.Select(e => e.Id));
        Assert.Equal([1, 2], viewModel.VisibleEvents.Select(e => e.Id));
        Assert.Equal(1, viewModel.LoadEventsAsyncCallCount);
    }

    [Fact]
    public void SetSearchText_UpdatesStateAndVisibleEvents()
    {
        var viewModel = CreateInitializedViewModel();

        viewModel.SetSearchText("concert");

        Assert.Equal("concert", viewModel.EventListState.SearchText);
        Assert.Equal([4], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public void SetSearchText_TreatsNullAsEmptyStringAndRestoresVisibleEvents()
    {
        var viewModel = CreateInitializedViewModel();
        viewModel.SetSearchText("concert");

        viewModel.SetSearchText(null);

        Assert.Equal(string.Empty, viewModel.EventListState.SearchText);
        Assert.Equal([3, 2, 4, 1], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public void SetSearchText_DoesNotRaiseVisibleEventsChangeWhenEffectiveValueIsUnchanged()
    {
        var viewModel = CreateInitializedViewModel();
        var propertyChanges = new List<string?>();
        viewModel.PropertyChanged += (_, args) => propertyChanges.Add(args.PropertyName);

        viewModel.SetSearchText(string.Empty);

        Assert.DoesNotContain(nameof(EventListPageViewModel.VisibleEvents), propertyChanges);
    }

    [Fact]
    public void SetSortOption_UpdatesStateAndVisibleEvents()
    {
        var viewModel = CreateInitializedViewModel();

        viewModel.SetSortOption(EventSortOption.PriceDescending);

        Assert.Equal(EventSortOption.PriceDescending, viewModel.EventListState.SelectedSortOption);
        Assert.Equal([4, 1, 2, 3], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public void SetSortOption_DoesNotRaiseVisibleEventsChangeWhenValueIsUnchanged()
    {
        var viewModel = CreateInitializedViewModel();
        var propertyChanges = new List<string?>();
        viewModel.PropertyChanged += (_, args) => propertyChanges.Add(args.PropertyName);

        viewModel.SetSortOption(EventSortOption.DateAscending);

        Assert.DoesNotContain(nameof(EventListPageViewModel.VisibleEvents), propertyChanges);
    }

    [Fact]
    public void UpdateFilters_AppliesMutationAndRefreshesVisibleEvents()
    {
        var viewModel = CreateInitializedViewModel();

        viewModel.UpdateFilters(filters =>
        {
            filters.LocationReference = "Hall A";
            filters.MinimumTicketPrice = 20;
        });

        Assert.Equal("Hall A", viewModel.EventListState.ActiveFilters.LocationReference);
        Assert.Equal(20m, viewModel.EventListState.ActiveFilters.MinimumTicketPrice);
        Assert.Equal([1], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public void UpdateFilters_ThrowsForNullDelegate()
    {
        var viewModel = CreateInitializedViewModel();

        Assert.Throws<ArgumentNullException>(() => viewModel.UpdateFilters(null!));
    }

    [Fact]
    public void ResetEventListState_ResetsStateAndRestoresVisibleEvents()
    {
        var viewModel = CreateInitializedViewModel();
        viewModel.SetSearchText("festival");
        viewModel.SetSortOption(EventSortOption.PriceDescending);
        viewModel.UpdateFilters(filters => filters.OnlyAvailableEvents = true);

        viewModel.ResetEventListState();

        Assert.Equal(string.Empty, viewModel.EventListState.SearchText);
        Assert.Equal(EventSortOption.DateAscending, viewModel.EventListState.SelectedSortOption);
        Assert.False(viewModel.EventListState.ActiveFilters.HasActiveFilters());
        Assert.Equal([3, 2, 4, 1], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public void RefreshVisibleEvents_RaisesPropertyChangedForVisibleEvents()
    {
        var viewModel = CreateInitializedViewModel();
        string? changedProperty = null;
        viewModel.PropertyChanged += (_, args) => changedProperty = args.PropertyName;

        viewModel.UpdateFilters(filters => filters.EventType = "Festival");

        Assert.Equal(nameof(EventListPageViewModel.VisibleEvents), changedProperty);
    }

    private static TestEventListPageViewModel CreateInitializedViewModel()
    {
        var viewModel = new TestEventListPageViewModel(BuildSampleEvents());
        viewModel.SetAllEventsForTest(BuildSampleEvents());
        viewModel.RefreshVisibleEvents();
        return viewModel;
    }

    private static IReadOnlyList<Event> BuildSampleEvents()
    {
        return
        [
            new Event
            {
                Id = 1,
                Title = "Festival Spotlight",
                Description = "Main evening screening.",
                PosterUrl = string.Empty,
                EventDateTime = DateTime.Now.AddDays(5),
                LocationReference = "Hall A",
                TicketPrice = 30,
                HistoricalRating = 4.6,
                EventType = "Premiere",
                MaxCapacity = 100,
                CurrentEnrollment = 60,
                CreatorUserId = 10,
            },
            new Event
            {
                Id = 2,
                Title = "Indie Festival Encore",
                Description = "Late night audience favorite.",
                PosterUrl = string.Empty,
                EventDateTime = DateTime.Now.AddDays(2),
                LocationReference = "Hall A",
                TicketPrice = 15,
                HistoricalRating = 4.8,
                EventType = "Festival",
                MaxCapacity = 80,
                CurrentEnrollment = 50,
                CreatorUserId = 11,
            },
            new Event
            {
                Id = 3,
                Title = "Archive Revival",
                Description = "Classic documentary presentation.",
                PosterUrl = string.Empty,
                EventDateTime = DateTime.Now.AddDays(-1),
                LocationReference = "Hall B",
                TicketPrice = 10,
                HistoricalRating = 4.2,
                EventType = "Screening",
                MaxCapacity = 50,
                CurrentEnrollment = 50,
                CreatorUserId = 12,
            },
            new Event
            {
                Id = 4,
                Title = "Open Air Concert",
                Description = "Music under the stars.",
                PosterUrl = string.Empty,
                EventDateTime = DateTime.Now.AddDays(3),
                LocationReference = "Rooftop",
                TicketPrice = 45,
                HistoricalRating = 4.9,
                EventType = "Concert",
                MaxCapacity = 200,
                CurrentEnrollment = 150,
                CreatorUserId = 13,
            },
        ];
    }

    private sealed class TestEventListPageViewModel : EventListPageViewModel
    {
        private readonly IReadOnlyList<Event> _eventsToLoad;

        public TestEventListPageViewModel(IReadOnlyList<Event> eventsToLoad)
        {
            _eventsToLoad = eventsToLoad;
        }

        public override string PageTitle => "Test Events";

        public int LoadEventsAsyncCallCount { get; private set; }

        public void SetAllEventsForTest(IReadOnlyList<Event> events)
        {
            AllEvents = events;
        }

        protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
        {
            LoadEventsAsyncCallCount++;
            return Task.FromResult(_eventsToLoad);
        }
    }
}
