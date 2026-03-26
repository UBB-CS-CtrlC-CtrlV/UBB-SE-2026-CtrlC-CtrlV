using MovieApp.Core.EventLists;
using MovieApp.Core.Models;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class EventListTests
{
    private readonly IReadOnlyList<Event> _sampleEvents = BuildSampleEvents();

    [Fact]
    public void EventFilterState_HasActiveFilters_ReturnsFalseForDefaults()
    {
        var filters = new EventFilterState();

        Assert.False(filters.HasActiveFilters());
    }

    [Fact]
    public void EventFilterState_HasActiveFilters_ReturnsTrueForWhitespaceInsensitiveValues()
    {
        var filters = new EventFilterState
        {
            EventType = "  Premiere  ",
        };

        Assert.True(filters.HasActiveFilters());
    }

    [Fact]
    public void EventFilterState_Clone_CreatesIndependentCopy()
    {
        var original = new EventFilterState
        {
            EventType = "Premiere",
            LocationReference = "Cluj",
            MinimumTicketPrice = 20,
            MaximumTicketPrice = 60,
            OnlyAvailableEvents = true,
        };

        var clone = original.Clone();
        clone.EventType = "Concert";
        clone.MinimumTicketPrice = 5;
        clone.OnlyAvailableEvents = false;

        Assert.Equal("Premiere", original.EventType);
        Assert.Equal(20m, original.MinimumTicketPrice);
        Assert.True(original.OnlyAvailableEvents);
    }

    [Fact]
    public void EventFilterState_Reset_ClearsAllFields()
    {
        var filters = new EventFilterState
        {
            EventType = "Premiere",
            LocationReference = "Cluj",
            MinimumTicketPrice = 10,
            MaximumTicketPrice = 100,
            OnlyAvailableEvents = true,
        };

        filters.Reset();

        Assert.Null(filters.EventType);
        Assert.Null(filters.LocationReference);
        Assert.Null(filters.MinimumTicketPrice);
        Assert.Null(filters.MaximumTicketPrice);
        Assert.False(filters.OnlyAvailableEvents);
    }

    [Fact]
    public void EventListState_Clone_CreatesIndependentNestedState()
    {
        var original = new EventListState
        {
            SearchText = "festival",
            SelectedSortOption = EventSortOption.PriceDescending,
            ActiveFilters = new EventFilterState
            {
                EventType = "Festival",
                OnlyAvailableEvents = true,
            },
        };

        var clone = original.Clone();
        clone.SearchText = "concert";
        clone.SelectedSortOption = EventSortOption.DateDescending;
        clone.ActiveFilters.EventType = "Concert";
        clone.ActiveFilters.OnlyAvailableEvents = false;

        Assert.Equal("festival", original.SearchText);
        Assert.Equal(EventSortOption.PriceDescending, original.SelectedSortOption);
        Assert.Equal("Festival", original.ActiveFilters.EventType);
        Assert.True(original.ActiveFilters.OnlyAvailableEvents);
    }

    [Fact]
    public void EventListState_Reset_RestoresDefaults()
    {
        var state = new EventListState
        {
            SearchText = "festival",
            SelectedSortOption = EventSortOption.PriceDescending,
            ActiveFilters = new EventFilterState
            {
                EventType = "Festival",
                MinimumTicketPrice = 15,
                OnlyAvailableEvents = true,
            },
        };

        state.Reset();

        Assert.Equal(string.Empty, state.SearchText);
        Assert.Equal(EventSortOption.DateAscending, state.SelectedSortOption);
        Assert.False(state.ActiveFilters.HasActiveFilters());
    }

    [Fact]
    public void ApplyFilters_FiltersByAvailabilityTypeLocationAndPrice()
    {
        var filters = new EventFilterState
        {
            EventType = "  premiere ",
            LocationReference = " hall a ",
            MinimumTicketPrice = 20,
            MaximumTicketPrice = 50,
            OnlyAvailableEvents = true,
        };

        var result = EventListTransformer.ApplyFilters(_sampleEvents, filters).Select(e => e.Id);

        Assert.Equal([1], result);
    }

    [Fact]
    public void ApplyFilters_InvalidPriceRange_ReturnsEmpty()
    {
        var filters = new EventFilterState
        {
            MinimumTicketPrice = 100,
            MaximumTicketPrice = 20,
        };

        var result = EventListTransformer.ApplyFilters(_sampleEvents, filters);

        Assert.Empty(result);
    }

    [Fact]
    public void ApplySearch_TrimsAndMatchesAcrossSupportedFields()
    {
        var byTitle = EventListTransformer.ApplySearch(_sampleEvents, "  spotlight ").Select(e => e.Id);
        var byDescription = EventListTransformer.ApplySearch(_sampleEvents, "documentary").Select(e => e.Id);
        var byLocation = EventListTransformer.ApplySearch(_sampleEvents, "rooftop").Select(e => e.Id);
        var byType = EventListTransformer.ApplySearch(_sampleEvents, "workshop").Select(e => e.Id);

        Assert.Equal([1], byTitle);
        Assert.Equal([3], byDescription);
        Assert.Equal([4], byLocation);
        Assert.Equal([5], byType);
    }

    [Fact]
    public void ApplySearch_MatchesTitleAndEventTypeCaseInsensitively()
    {
        var byTitle = EventListTransformer.ApplySearch(_sampleEvents, "FESTIVAL").Select(e => e.Id);
        var byType = EventListTransformer.ApplySearch(_sampleEvents, "PREMIERE").Select(e => e.Id);

        Assert.Equal([1, 2], byTitle);
        Assert.Equal([1], byType);
    }

    [Fact]
    public void ApplySearch_FiltersOnlyTheProvidedSequence()
    {
        var homeEvents = _sampleEvents.Take(2).ToList();
        var sectionEvents = _sampleEvents.Skip(2).ToList();

        var homeResult = EventListTransformer.ApplySearch(homeEvents, "festival").Select(e => e.Id);
        var sectionResult = EventListTransformer.ApplySearch(sectionEvents, "festival").Select(e => e.Id);

        Assert.Equal([1, 2], homeResult);
        Assert.Empty(sectionResult);
    }

    [Fact]
    public void ApplySorting_SortsByEachSupportedOption()
    {
        Assert.Equal([3, 2, 4, 5, 1], EventListTransformer.ApplySorting(_sampleEvents, EventSortOption.DateAscending).Select(e => e.Id));
        Assert.Equal([1, 5, 4, 2, 3], EventListTransformer.ApplySorting(_sampleEvents, EventSortOption.DateDescending).Select(e => e.Id));
        Assert.Equal([3, 2, 1, 4, 5], EventListTransformer.ApplySorting(_sampleEvents, EventSortOption.PriceAscending).Select(e => e.Id));
        Assert.Equal([5, 4, 1, 2, 3], EventListTransformer.ApplySorting(_sampleEvents, EventSortOption.PriceDescending).Select(e => e.Id));
        Assert.Equal([4, 2, 1, 3, 5], EventListTransformer.ApplySorting(_sampleEvents, EventSortOption.HistoricalRatingDescending).Select(e => e.Id));
    }

    [Fact]
    public void Apply_RunsFilterSearchSortPipeline()
    {
        var state = new EventListState
        {
            SearchText = "festival",
            SelectedSortOption = EventSortOption.PriceDescending,
            ActiveFilters = new EventFilterState
            {
                LocationReference = "hall a",
                MinimumTicketPrice = 10,
            },
        };

        var result = EventListTransformer.Apply(_sampleEvents, state).Select(e => e.Id);

        Assert.Equal([1, 2], result);
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
                PosterUrl = "",
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
                PosterUrl = "",
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
                PosterUrl = "",
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
                PosterUrl = "",
                EventDateTime = DateTime.Now.AddDays(3),
                LocationReference = "Rooftop",
                TicketPrice = 45,
                HistoricalRating = 4.9,
                EventType = "Concert",
                MaxCapacity = 200,
                CurrentEnrollment = 150,
                CreatorUserId = 13,
            },
            new Event
            {
                Id = 5,
                Title = "Directing Masterclass",
                Description = "Hands-on session with guest director.",
                PosterUrl = "",
                EventDateTime = DateTime.Now.AddDays(4),
                LocationReference = "Studio 3",
                TicketPrice = 60,
                HistoricalRating = 4.0,
                EventType = "Workshop",
                MaxCapacity = 30,
                CurrentEnrollment = 5,
                CreatorUserId = 14,
            },
        ];
    }
}
