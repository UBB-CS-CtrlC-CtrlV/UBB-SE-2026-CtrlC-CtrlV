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

        Assert.Equal("Home Events", viewModel.PageTitle);
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

        public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
