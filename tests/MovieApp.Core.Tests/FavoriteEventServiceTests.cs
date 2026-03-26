using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Core.Tests.Fakes;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class FavoriteEventServiceTests
{
    private readonly FakeFavoriteEventRepository _favoriteRepository = new();
    private readonly FakeEventRepository _eventRepository = new();
    private readonly FavoriteEventService _service;

    public FavoriteEventServiceTests()
    {
        _service = new FavoriteEventService(_favoriteRepository, _eventRepository);
    }

    [Fact]
    public async Task AddFavoriteAsync_AddsPersistedEventToFavorites()
    {
        _eventRepository.Items.Add(new Event { Id = 1, Title = "Test Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });

        await _service.AddFavoriteAsync(100, 1);

        var favorites = await _service.GetFavoritesByUserAsync(100);
        Assert.Single(favorites);
        Assert.Equal(1, favorites[0].EventId);
    }

    [Fact]
    public async Task AddFavoriteAsync_ThrowsWhenFavoriteAlreadyExists()
    {
        _eventRepository.Items.Add(new Event { Id = 1, Title = "Test Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });
        await _service.AddFavoriteAsync(100, 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AddFavoriteAsync(100, 1));
    }

    [Fact]
    public async Task RemoveFavoriteAsync_RemovesFavoriteLink()
    {
        _eventRepository.Items.Add(new Event { Id = 1, Title = "Test Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });
        await _service.AddFavoriteAsync(100, 1);

        await _service.RemoveFavoriteAsync(100, 1);

        var favorites = await _service.GetFavoritesByUserAsync(100);
        Assert.Empty(favorites);
    }

    [Fact]
    public async Task GetFavoriteEventsByUserIdAsync_ReturnsResolvedEvents()
    {
        _eventRepository.Items.Add(new Event { Id = 10, Title = "Test", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });
        await _favoriteRepository.AddAsync(1, 10);

        var result = await _service.GetFavoriteEventsByUserIdAsync(1);

        var favoriteEvent = Assert.Single(result);
        Assert.Equal(10, favoriteEvent.Id);
    }
}
