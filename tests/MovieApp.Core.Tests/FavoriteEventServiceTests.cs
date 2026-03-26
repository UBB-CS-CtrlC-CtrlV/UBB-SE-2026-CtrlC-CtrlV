<<<<<<< Updated upstream
using System;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Core.Tests.Fakes;
using Xunit;

namespace MovieApp.Core.Tests;

public class FavoriteEventServiceTests
{
    private readonly FakeFavoriteEventRepository _favoriteRepo;
    private readonly FakeEventRepository _eventRepo;
=======
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests.Services;

public class FavoriteEventServiceTests
{
    private readonly Mock<IFavoriteEventRepository> _favoriteRepoMock;
    private readonly Mock<IEventRepository> _eventRepoMock;
>>>>>>> Stashed changes
    private readonly FavoriteEventService _sut;

    public FavoriteEventServiceTests()
    {
<<<<<<< Updated upstream
        _favoriteRepo = new FakeFavoriteEventRepository();
        _eventRepo = new FakeEventRepository();
        _sut = new FavoriteEventService(_favoriteRepo, _eventRepo);
    }

    [Fact]
    public async Task AddFavoriteAsync_AddsToRepository()
    {
        _eventRepo.Items.Add(new Event { Id = 1, Title = "Test Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });

        await _sut.AddFavoriteAsync(100, 1);

        var favorites = await _sut.GetFavoritesByUserAsync(100);
        Assert.Single(favorites);
        Assert.Equal(1, favorites[0].EventId);
    }

    [Fact]
    public async Task AddFavoriteAsync_DuplicateThrowsException()
    {
        _eventRepo.Items.Add(new Event { Id = 1, Title = "Test Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });
        await _sut.AddFavoriteAsync(100, 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddFavoriteAsync(100, 1));
    }

    [Fact]
    public async Task RemoveFavoriteAsync_RemovesFromRepository()
    {
        _eventRepo.Items.Add(new Event { Id = 1, Title = "Test Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });
        await _sut.AddFavoriteAsync(100, 1);

        await _sut.RemoveFavoriteAsync(100, 1);

        var favorites = await _sut.GetFavoritesByUserAsync(100);
        Assert.Empty(favorites);
=======
        _favoriteRepoMock = new Mock<IFavoriteEventRepository>();
        _eventRepoMock = new Mock<IEventRepository>();
        _sut = new FavoriteEventService(_favoriteRepoMock.Object, _eventRepoMock.Object);
    }

    [Fact]
    public async Task AddFavoriteAsync_WhenNotAlreadyFavorited_ShouldAdd()
    {
        // Arrange
        var userId = 1;
        var eventId = 10;
        _favoriteRepoMock.Setup(x => x.FindByUserAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<FavoriteEvent>());

        // Act
        await _sut.AddFavoriteAsync(userId, eventId);

        // Assert
        _favoriteRepoMock.Verify(x => x.AddAsync(userId, eventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddFavoriteAsync_WhenAlreadyFavorited_ShouldNotAdd()
    {
        // Arrange
        var userId = 1;
        var eventId = 10;
        _favoriteRepoMock.Setup(x => x.FindByUserAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<FavoriteEvent> { new FavoriteEvent { Id = 1, UserId = userId, EventId = eventId } });

        // Act
        await _sut.AddFavoriteAsync(userId, eventId);

        // Assert
        _favoriteRepoMock.Verify(x => x.AddAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveFavoriteAsync_ShouldCallRepository()
    {
        // Arrange
        var userId = 1;
        var eventId = 10;

        // Act
        await _sut.RemoveFavoriteAsync(userId, eventId);

        // Assert
        _favoriteRepoMock.Verify(x => x.RemoveAsync(userId, eventId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFavoriteEventsByUserIdAsync_ShouldReturnEvents()
    {
        // Arrange
        var userId = 1;
        var eventId = 10;
        _favoriteRepoMock.Setup(x => x.FindByUserAsync(userId, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new List<FavoriteEvent> { new FavoriteEvent { Id = 1, UserId = userId, EventId = eventId } });

        var ev = new Event
        {
            Id = eventId,
            Title = "Test",
            EventDateTime = System.DateTime.Now,
            LocationReference = "Loc",
            CreatorUserId = 1,
            TicketPrice = 10
        };

        _eventRepoMock.Setup(x => x.FindByIdAsync(eventId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(ev);

        // Act
        var result = await _sut.GetFavoriteEventsByUserIdAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal(eventId, result[0].Id);
>>>>>>> Stashed changes
    }
}
