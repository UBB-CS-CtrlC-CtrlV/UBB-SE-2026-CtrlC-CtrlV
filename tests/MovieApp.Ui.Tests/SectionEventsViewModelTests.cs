using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using Microsoft.UI.Xaml;
using MovieApp.Ui.ViewModels.Events;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class SectionEventsViewModelTests
{
    [Fact]
    public async Task InitializeAsync_LoadsOnlyEventsMatchingSelectedSection()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var context = new SectionNavigationContext
        {
            Title = "Premiere",
            GroupingValue = "Premiere",
        };

        var viewModel = new SectionEventsViewModel(repository, context);

        await viewModel.InitializeAsync();

        Assert.Equal("Premiere", viewModel.PageTitle);
        Assert.Equal([1, 2], viewModel.AllEvents.Select(e => e.Id));
        Assert.Equal([1, 2], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public async Task InitializeAsync_MatchesSectionCaseInsensitiveAndTrimmed()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var context = new SectionNavigationContext
        {
            Title = "Premiere",
            GroupingValue = " premiere ",
        };

        var viewModel = new SectionEventsViewModel(repository, context);

        await viewModel.InitializeAsync();

        Assert.Equal([1, 2], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public async Task InitializeAsync_IgnoresEventsWithoutValidType()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var context = new SectionNavigationContext
        {
            Title = "Premiere",
            GroupingValue = "Premiere",
        };

        var viewModel = new SectionEventsViewModel(repository, context);

        await viewModel.InitializeAsync();

        Assert.DoesNotContain(viewModel.VisibleEvents, e => e.Id is 4 or 5);
    }

    [Fact]
    public async Task SetSearchText_KeepsFilteringInsideSelectedSection()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var context = new SectionNavigationContext
        {
            Title = "Premiere",
            GroupingValue = "Premiere",
        };

        var viewModel = new SectionEventsViewModel(repository, context);

        await viewModel.InitializeAsync();
        viewModel.SetSearchText("beta");

        Assert.Equal([2], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public async Task SetSearchText_MatchesEventTypeOnlyInsideSelectedSection()
    {
        var repository = new StubEventRepository(BuildSampleEvents());
        var context = new SectionNavigationContext
        {
            Title = "Premiere",
            GroupingValue = "Premiere",
        };

        var viewModel = new SectionEventsViewModel(repository, context);

        await viewModel.InitializeAsync();
        viewModel.SetSearchText("premiere");

        Assert.Equal([1, 2], viewModel.VisibleEvents.Select(e => e.Id));
    }

    [Fact]
    public async Task InitializeAsync_WithoutRepository_ShowsUnavailableStateAndNoEvents()
    {
        var context = new SectionNavigationContext
        {
            Title = "Premiere",
            GroupingValue = "Premiere",
        };
        var viewModel = new SectionEventsViewModel(null, context);

        await viewModel.InitializeAsync();

        Assert.False(viewModel.IsRepositoryAvailable);
        Assert.Equal(Visibility.Visible, viewModel.UnavailableMessageVisibility);
        Assert.Empty(viewModel.AllEvents);
        Assert.Empty(viewModel.VisibleEvents);
    }

    private static IReadOnlyList<Event> BuildSampleEvents()
    {
        return
        [
            new Event
            {
                Id = 1,
                Title = "Premiere Alpha",
                Description = "First premiere",
                PosterUrl = string.Empty,
                EventDateTime = new DateTime(2030, 1, 1, 18, 0, 0),
                LocationReference = "Hall A",
                TicketPrice = 20,
                HistoricalRating = 4.5,
                EventType = "Premiere",
                MaxCapacity = 100,
                CurrentEnrollment = 10,
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 2,
                Title = "Premiere Beta",
                Description = "Second premiere",
                PosterUrl = string.Empty,
                EventDateTime = new DateTime(2030, 1, 2, 18, 0, 0),
                LocationReference = "Hall B",
                TicketPrice = 25,
                HistoricalRating = 4.2,
                EventType = " Premiere ",
                MaxCapacity = 100,
                CurrentEnrollment = 12,
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
                Description = "Whitespace event type",
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
            new Event
            {
                Id = 5,
                Title = "Unknown Type",
                Description = "Null type",
                PosterUrl = string.Empty,
                EventDateTime = new DateTime(2030, 1, 5, 18, 0, 0),
                LocationReference = "Hall E",
                TicketPrice = 18,
                HistoricalRating = 3.8,
                EventType = null!,
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
            return Task.FromResult<IEnumerable<Event>>(
                _events.Where(e => string.Equals(e.EventType, eventType, StringComparison.OrdinalIgnoreCase)).ToList());
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
