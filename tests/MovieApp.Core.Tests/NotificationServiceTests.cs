<<<<<<< Updated upstream
using System.Threading.Tasks;
using MovieApp.Core.Tests.Fakes;
using MovieApp.Core.Services;
using Xunit;
using System.Linq;

namespace MovieApp.Core.Tests;

public class NotificationServiceTests
{
    private readonly FakeNotificationRepository _notificationRepo;
    private readonly FakeFavoriteEventRepository _favoriteRepo;
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

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepoMock;
    private readonly Mock<IFavoriteEventRepository> _favoriteRepoMock;
    private readonly Mock<IEventRepository> _eventRepoMock;
>>>>>>> Stashed changes
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
<<<<<<< Updated upstream
        _notificationRepo = new FakeNotificationRepository();
        _favoriteRepo = new FakeFavoriteEventRepository();
        _sut = new NotificationService(_notificationRepo, _favoriteRepo);
    }

    [Fact]
    public async Task GeneratePriceDropNotificationAsync_NotifiesFavoritedUsers()
    {
        await _favoriteRepo.AddAsync(100, 1);
        await _favoriteRepo.AddAsync(101, 1);
        await _favoriteRepo.AddAsync(102, 2); // Different event

        await _sut.GeneratePriceDropNotificationAsync(1, "Test Event");

        var user100Notifs = await _sut.GetNotificationsByUserAsync(100);
        Assert.Single(user100Notifs);
        Assert.Equal("PRICE_DROP", user100Notifs[0].Type);

        var user101Notifs = await _sut.GetNotificationsByUserAsync(101);
        Assert.Single(user101Notifs);

        var user102Notifs = await _sut.GetNotificationsByUserAsync(102);
        Assert.Empty(user102Notifs);
    }

    [Fact]
    public async Task GenerateSeatsAvailableNotificationAsync_AvoidsDuplicates()
    {
        await _favoriteRepo.AddAsync(100, 1);

        await _sut.GenerateSeatsAvailableNotificationAsync(1, "Test Event");
        await _sut.GenerateSeatsAvailableNotificationAsync(1, "Test Event");

        var user100Notifs = await _sut.GetNotificationsByUserAsync(100);
        Assert.Single(user100Notifs); // Still 1 because it prevents duplicates
=======
        _notificationRepoMock = new Mock<INotificationRepository>();
        _favoriteRepoMock = new Mock<IFavoriteEventRepository>();
        _eventRepoMock = new Mock<IEventRepository>();

        _sut = new NotificationService(_notificationRepoMock.Object, _favoriteRepoMock.Object, _eventRepoMock.Object);
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenPriceDrops_ShouldNotifiyFavoritedUsers()
    {
        // Arrange
        var eventId = 10;
        var oldPrice = 20m;
        var newPrice = 10m; // Price dropped

        _eventRepoMock.Setup(x => x.FindByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = eventId, Title = "Event", CreatorUserId = 1, EventDateTime = System.DateTime.Now, LocationReference = "A", TicketPrice = newPrice });

        var userId = 100;
        _favoriteRepoMock.Setup(x => x.FindByEventAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FavoriteEvent> { new FavoriteEvent { Id = 1, EventId = eventId, UserId = userId } });

        _notificationRepoMock.Setup(x => x.FindByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification>()); // no existing unread price drop

        // Act
        await _sut.NotifyPriceDropAsync(eventId, oldPrice, newPrice);

        // Assert
        _notificationRepoMock.Verify(x => x.AddAsync(It.Is<Notification>(n => n.UserId == userId && n.Type == "PriceDrop"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyPriceDropAsync_WhenDuplicateUnreadExists_ShouldAvoidDuplicates()
    {
        // Arrange
        var eventId = 10;
        var oldPrice = 20m;
        var newPrice = 10m;

        _eventRepoMock.Setup(x => x.FindByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = eventId, Title = "Event", CreatorUserId = 1, EventDateTime = System.DateTime.Now, LocationReference = "A", TicketPrice = newPrice });

        var userId = 100;
        _favoriteRepoMock.Setup(x => x.FindByEventAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FavoriteEvent> { new FavoriteEvent { Id = 1, EventId = eventId, UserId = userId } });

        // Existing unread price drop notification
        _notificationRepoMock.Setup(x => x.FindByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification> { new Notification { Id = 99, UserId = userId, EventId = eventId, Type = "PriceDrop", Message = "test", State = NotificationState.Unread } });

        // Act
        await _sut.NotifyPriceDropAsync(eventId, oldPrice, newPrice);

        // Assert
        // Should not add another identical unread notification
        _notificationRepoMock.Verify(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NotifySeatsAvailableAsync_WhenSeatsBecomeAvailable_ShouldNotifyFavoritedUsers()
    {
        // Arrange
        var eventId = 10;
        
        // Let's assume Capacity is 50, Enrollment is 49. It means 1 seat is now available.
        // It was full previously perhaps.
        var newCapacity = 50;
        var enrollment = 49;

        _eventRepoMock.Setup(x => x.FindByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = eventId, Title = "Event", CreatorUserId = 1, EventDateTime = System.DateTime.Now, LocationReference = "A", TicketPrice = 10, MaxCapacity = newCapacity, CurrentEnrollment = enrollment });

        var userId = 100;
        _favoriteRepoMock.Setup(x => x.FindByEventAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FavoriteEvent> { new FavoriteEvent { Id = 1, EventId = eventId, UserId = userId } });

        _notificationRepoMock.Setup(x => x.FindByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification>());

        // Act
        await _sut.NotifySeatsAvailableAsync(eventId, newCapacity);

        // Assert
        _notificationRepoMock.Verify(x => x.AddAsync(It.Is<Notification>(n => n.UserId == userId && n.Type == "SeatsAvailable"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifySeatsAvailableAsync_WhenDuplicateUnreadExists_ShouldAvoidDuplicates()
    {
        // Arrange
        var eventId = 10;
        var newCapacity = 50;
        var enrollment = 49;

        _eventRepoMock.Setup(x => x.FindByIdAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Event { Id = eventId, Title = "Event", CreatorUserId = 1, EventDateTime = System.DateTime.Now, LocationReference = "A", TicketPrice = 10, MaxCapacity = newCapacity, CurrentEnrollment = enrollment });

        var userId = 100;
        _favoriteRepoMock.Setup(x => x.FindByEventAsync(eventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FavoriteEvent> { new FavoriteEvent { Id = 1, EventId = eventId, UserId = userId } });

        _notificationRepoMock.Setup(x => x.FindByUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Notification> { new Notification { Id = 99, UserId = userId, EventId = eventId, Type = "SeatsAvailable", Message = "test", State = NotificationState.Unread } });

        // Act
        await _sut.NotifySeatsAvailableAsync(eventId, newCapacity);

        // Assert
        _notificationRepoMock.Verify(x => x.AddAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()), Times.Never);
>>>>>>> Stashed changes
    }
}
