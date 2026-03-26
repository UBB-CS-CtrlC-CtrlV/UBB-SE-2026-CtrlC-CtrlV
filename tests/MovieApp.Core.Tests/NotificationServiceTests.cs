using MovieApp.Core.Models;
using MovieApp.Core.Services;
using MovieApp.Core.Tests.Fakes;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class NotificationServiceTests
{
    [Fact]
    public async Task GeneratePriceDropNotificationAsync_NotifiesFavoritedUsers()
    {
        var notificationRepository = new FakeNotificationRepository();
        var favoriteRepository = new FakeFavoriteEventRepository();
        var service = new NotificationService(notificationRepository, favoriteRepository);

        await favoriteRepository.AddAsync(100, 1);
        await favoriteRepository.AddAsync(101, 1);
        await favoriteRepository.AddAsync(102, 2);

        await service.GeneratePriceDropNotificationAsync(1, "Test Event");

        Assert.Single(await service.GetNotificationsByUserAsync(100));
        Assert.Single(await service.GetNotificationsByUserAsync(101));
        Assert.Empty(await service.GetNotificationsByUserAsync(102));
    }

    [Fact]
    public async Task GenerateSeatsAvailableNotificationAsync_AvoidsDuplicateUnreadNotifications()
    {
        var notificationRepository = new FakeNotificationRepository();
        var favoriteRepository = new FakeFavoriteEventRepository();
        var service = new NotificationService(notificationRepository, favoriteRepository);

        await favoriteRepository.AddAsync(100, 1);

        await service.GenerateSeatsAvailableNotificationAsync(1, "Test Event");
        await service.GenerateSeatsAvailableNotificationAsync(1, "Test Event");

        Assert.Single(await service.GetNotificationsByUserAsync(100));
    }

    [Fact]
    public async Task NotifyPriceDropAsync_AddsTypedNotificationWhenPriceFalls()
    {
        var notificationRepository = new FakeNotificationRepository();
        var favoriteRepository = new FakeFavoriteEventRepository();
        var eventRepository = new FakeEventRepository();
        var service = new NotificationService(notificationRepository, favoriteRepository, eventRepository);

        eventRepository.Items.Add(new Event { Id = 10, Title = "Event", EventDateTime = DateTime.Now, LocationReference = "Loc", TicketPrice = 10, CreatorUserId = 1 });
        await favoriteRepository.AddAsync(100, 10);

        await service.NotifyPriceDropAsync(10, oldPrice: 20, newPrice: 10);

        var notification = Assert.Single(await service.GetNotificationsByUserIdAsync(100));
        Assert.Equal("PriceDrop", notification.Type);
    }

    [Fact]
    public async Task NotifySeatsAvailableAsync_AvoidsDuplicateUnreadNotifications()
    {
        var notificationRepository = new FakeNotificationRepository();
        var favoriteRepository = new FakeFavoriteEventRepository();
        var eventRepository = new FakeEventRepository();
        var service = new NotificationService(notificationRepository, favoriteRepository, eventRepository);

        eventRepository.Items.Add(new Event
        {
            Id = 10,
            Title = "Event",
            EventDateTime = DateTime.Now,
            LocationReference = "Loc",
            TicketPrice = 10,
            CreatorUserId = 1,
            MaxCapacity = 50,
            CurrentEnrollment = 49,
        });
        await favoriteRepository.AddAsync(100, 10);

        await service.NotifySeatsAvailableAsync(10, 50);
        await service.NotifySeatsAvailableAsync(10, 50);

        var notification = Assert.Single(await service.GetNotificationsByUserIdAsync(100));
        Assert.Equal("SeatsAvailable", notification.Type);
    }
}
