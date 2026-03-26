using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.ViewModels.Events;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class HomeEventsViewModelTests
{
    [Fact]
    public async Task InitializeAsync_SetsIsLoadingTrueWhileLoadingAndFalseAfter()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        Assert.False(viewModel.IsLoading);

        await viewModel.InitializeAsync();

        Assert.False(viewModel.IsLoading);
    }

    [Fact]
    public async Task InitializeAsync_PopulatesAllEventsAndVisibleEvents()
    {
        var sampleEvents = BuildSampleEvents();
        var repository = new StubEventRepository(sampleEvents);
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();

        Assert.Equal(sampleEvents.Count, viewModel.AllEvents.Count);
        Assert.Equal(sampleEvents.Count, viewModel.VisibleEvents.Count);
    }

    [Fact]
    public async Task InitializeAsync_SetsHasNoEventsTrue_WhenRepositoryReturnsEmpty()
    {
        var repository = new StubEventRepository([]);
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();

        Assert.True(viewModel.HasNoEvents);
        Assert.False(viewModel.ShowEventList);
    }

    [Fact]
    public async Task InitializeAsync_SetsHasNoEventsFalse_WhenRepositoryReturnsEvents()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();

        Assert.False(viewModel.HasNoEvents);
        Assert.True(viewModel.ShowEventList);
    }

    [Fact]
    public async Task RefreshVisibleEvents_UpdatesListAfterInitialization()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();
        viewModel.SetSearchText("Festival");

        Assert.Equal(2, viewModel.VisibleEvents.Count);
        Assert.All(viewModel.VisibleEvents, e => Assert.Contains("Festival", e.Title));
    }

    [Fact]
    public void PageTitle_ReturnsHomeEvents()
    {
        var repository = new StubEventRepository([]);
        var viewModel = new HomeEventsViewModel(repository);

        Assert.Equal("Home", viewModel.PageTitle);
    }

    [Fact]
    public async Task InitializeAsync_RaisesPropertyChangedForIsLoadingAndShowEventList()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        await viewModel.InitializeAsync();

        Assert.Contains(nameof(EventListPageViewModel.IsLoading), changedProperties);
        Assert.Contains(nameof(EventListPageViewModel.ShowEventList), changedProperties);
    }

    [Fact]
    public async Task InitializeAsync_RaisesPropertyChangedForHasNoEvents_WhenRepositoryReturnsEmpty()
    {
        var repository = new StubEventRepository([]);
        var viewModel = new HomeEventsViewModel(repository);
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        await viewModel.InitializeAsync();

        Assert.Contains(nameof(EventListPageViewModel.HasNoEvents), changedProperties);
    }

    [Fact]
    public async Task InitializeAsync_BuildsSectionsGroupedByEventType()
    {
        var repository = new StubEventRepository(BuildSectionSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();

        Assert.Equal(["Festival", "Premiere"], viewModel.Sections.Select(s => s.Title));
        Assert.Equal([3], viewModel.Sections[0].Events.Select(e => e.Id));
        Assert.Equal([2, 1], viewModel.Sections[1].Events.Select(e => e.Id));
    }

    [Fact]
    public async Task SetSearchText_RebuildsSectionsFromVisibleEvents()
    {
        var repository = new StubEventRepository(BuildSectionSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();
        viewModel.SetSearchText("premiere");

        Assert.Equal(["Premiere"], viewModel.Sections.Select(s => s.Title));
        Assert.Equal([2, 1], viewModel.Sections[0].Events.Select(e => e.Id));
    }

    [Fact]
    public async Task SetSearchText_MatchesSectionsByEventType()
    {
        var repository = new StubEventRepository(BuildSectionSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();
        viewModel.SetSearchText("festival");

        var section = Assert.Single(viewModel.Sections);
        Assert.Equal("Festival", section.Title);
        Assert.Equal([3], section.Events.Select(e => e.Id));
    }

    [Fact]
    public async Task SetSortOption_PreservesSharedSortOrderInsideHomeSections()
    {
        var repository = new StubEventRepository(BuildSectionSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();
        viewModel.SetSortOption(MovieApp.Core.EventLists.EventSortOption.HistoricalRatingDescending);

        Assert.Equal(["Festival", "Premiere"], viewModel.Sections.Select(s => s.Title));
        Assert.Equal([3], viewModel.Sections[0].Events.Select(e => e.Id));
        Assert.Equal([1, 2], viewModel.Sections[1].Events.Select(e => e.Id));
    }

    [Fact]
    public async Task CreateNavigationContext_ReturnsSelectedGroupingInformation()
    {
        var repository = new StubEventRepository(BuildSectionSampleEvents());
        var viewModel = new HomeEventsViewModel(repository);

        await viewModel.InitializeAsync();

        var section = Assert.Single(viewModel.Sections, s => s.Title == "Premiere");
        var context = viewModel.CreateNavigationContext(section);

        Assert.Equal("Premiere", context.Title);
        Assert.Equal("Premiere", context.GroupingValue);
    }

    [Fact]
    public void NavigationShortcuts_ExposeHomeLinksToMyEventsAndEventManagement()
    {
        var repository = new StubEventRepository([]);
        var viewModel = new HomeEventsViewModel(repository);

        Assert.Equal(["My Events", "Event Management"], viewModel.NavigationShortcuts.Select(s => s.Title));
        Assert.Equal(["MyEvents", "EventManagement"], viewModel.NavigationShortcuts.Select(s => s.RouteTag));
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
        ];
    }

    private static IReadOnlyList<Event> BuildSectionSampleEvents()
    {
        return
        [
            new Event
            {
                Id = 1,
                Title = "Premiere A",
                Description = "First premiere",
                PosterUrl = string.Empty,
                EventDateTime = new DateTime(2030, 1, 2, 18, 0, 0),
                LocationReference = "Hall A",
                TicketPrice = 20,
                HistoricalRating = 4.5,
                EventType = " Premiere ",
                MaxCapacity = 100,
                CurrentEnrollment = 10,
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 2,
                Title = "Premiere B",
                Description = "Second premiere",
                PosterUrl = string.Empty,
                EventDateTime = new DateTime(2030, 1, 1, 18, 0, 0),
                LocationReference = "Hall B",
                TicketPrice = 25,
                HistoricalRating = 4.0,
                EventType = "Premiere",
                MaxCapacity = 100,
                CurrentEnrollment = 10,
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 3,
                Title = "Festival Night",
                Description = "Festival event",
                PosterUrl = string.Empty,
                EventDateTime = new DateTime(2030, 1, 3, 18, 0, 0),
                LocationReference = "Hall C",
                TicketPrice = 30,
                HistoricalRating = 4.9,
                EventType = "Festival",
                MaxCapacity = 100,
                CurrentEnrollment = 10,
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 4,
                Title = "Mystery Event",
                Description = "No event type",
                PosterUrl = string.Empty,
                EventDateTime = new DateTime(2030, 1, 4, 18, 0, 0),
                LocationReference = "Hall D",
                TicketPrice = 15,
                HistoricalRating = 3.5,
                EventType = "   ",
                MaxCapacity = 100,
                CurrentEnrollment = 10,
                CreatorUserId = 1,
            },
        ];
    }

    private sealed class StubEventRepository : IEventRepository
    {
        private readonly IReadOnlyList<Event> _events;

        public StubEventRepository(IReadOnlyList<Event> events)
        {
            _events = events;
        }

        public Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Event>>(_events);
        }

        public Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Event>>(_events.Where(e => e.EventType == eventType).ToList());
        }

        public Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_events.FirstOrDefault(e => e.Id == id));
        }

        public Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }

        public Task<bool> UpdateAsync(Event @event, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
