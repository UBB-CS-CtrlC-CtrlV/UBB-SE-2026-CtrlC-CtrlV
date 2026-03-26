using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IFavoriteEventRepository _favoriteEventRepository;
<<<<<<< Updated upstream

    public NotificationService(INotificationRepository notificationRepository, IFavoriteEventRepository favoriteEventRepository)
    {
        _notificationRepository = notificationRepository;
        _favoriteEventRepository = favoriteEventRepository;
    }

    public async Task GeneratePriceDropNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
    {
        await GenerateNotificationForFavoritesAsync(eventId, "PRICE_DROP", $"The ticket price for '{eventTitle}' has dropped!", cancellationToken);
    }

    public async Task GenerateSeatsAvailableNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
    {
        await GenerateNotificationForFavoritesAsync(eventId, "SEATS_AVAILABLE", $"Seats are now available for '{eventTitle}'!", cancellationToken);
    }

    private async Task GenerateNotificationForFavoritesAsync(int eventId, string type, string message, CancellationToken cancellationToken)
    {
        var favoritedUsers = await _favoriteEventRepository.GetUsersByFavoriteEventAsync(eventId, cancellationToken);
        
        foreach (var userId in favoritedUsers)
        {
            var notifications = await _notificationRepository.FindByUserAsync(userId, cancellationToken);
            var recentNotification = notifications.FirstOrDefault(n => n.EventId == eventId && n.Type == type);
            
            if (recentNotification != null && recentNotification.State == NotificationState.Unread)
=======
    private readonly IEventRepository _eventRepository;

    public NotificationService(
        INotificationRepository notificationRepository,
        IFavoriteEventRepository favoriteEventRepository,
        IEventRepository eventRepository)
    {
        _notificationRepository = notificationRepository;
        _favoriteEventRepository = favoriteEventRepository;
        _eventRepository = eventRepository;
    }

    public Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.FindByUserAsync(userId, cancellationToken);
    }

    public async Task NotifyPriceDropAsync(int eventId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default)
    {
        if (newPrice >= oldPrice)
        {
            return; // Not a price drop
        }

        var @event = await _eventRepository.FindByIdAsync(eventId, cancellationToken);
        if (@event is null)
        {
            return;
        }

        var favorites = await _favoriteEventRepository.FindByEventAsync(eventId, cancellationToken);
        if (favorites.Count == 0)
        {
            return;
        }

        foreach (var favorite in favorites)
        {
            // Avoid duplicate unread price drop notifications for the same event
            var userNotifications = await _notificationRepository.FindByUserAsync(favorite.UserId, cancellationToken);
            if (userNotifications.Any(n => n.EventId == eventId && n.Type == "PriceDrop" && n.State == NotificationState.Unread))
>>>>>>> Stashed changes
            {
                continue;
            }

            var notification = new Notification
            {
                Id = 0,
<<<<<<< Updated upstream
                UserId = userId,
                EventId = eventId,
                Type = type,
                Message = message,
=======
                UserId = favorite.UserId,
                EventId = eventId,
                Type = "PriceDrop",
                Message = $"The price for '{@event.Title}' has dropped to {newPrice:C}!",
>>>>>>> Stashed changes
                State = NotificationState.Unread,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);
        }
    }

<<<<<<< Updated upstream
    public async Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _notificationRepository.FindByUserAsync(userId, cancellationToken);
    }

    public async Task RemoveNotificationAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        await _notificationRepository.RemoveAsync(notificationId, cancellationToken);
=======
    public async Task NotifySeatsAvailableAsync(int eventId, int newCapacity, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.FindByIdAsync(eventId, cancellationToken);
        if (@event is null)
        {
            return;
        }

        // Only notify if seats became available (capacity > enrollment)
        // Wait, @event.CurrentEnrollment may not be fully synced depending on caller order,
        // but let's assume `newCapacity > @event.CurrentEnrollment`.
        if (newCapacity <= @event.CurrentEnrollment)
        {
            return;
        }

        var favorites = await _favoriteEventRepository.FindByEventAsync(eventId, cancellationToken);
        if (favorites.Count == 0)
        {
            return;
        }

        foreach (var favorite in favorites)
        {
            var userNotifications = await _notificationRepository.FindByUserAsync(favorite.UserId, cancellationToken);
            if (userNotifications.Any(n => n.EventId == eventId && n.Type == "SeatsAvailable" && n.State == NotificationState.Unread))
            {
                continue;
            }

            var notification = new Notification
            {
                Id = 0,
                UserId = favorite.UserId,
                EventId = eventId,
                Type = "SeatsAvailable",
                Message = $"Seats are now available for '{@event.Title}'!",
                State = NotificationState.Unread,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);
        }
    }

    public Task MarkAsReadOrRemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        // The requirements say users should be able to get their notifications
        // Maybe removing them is fine for "MarkAsReadOrRemoveAsync"
        return _notificationRepository.RemoveAsync(notificationId, cancellationToken);
>>>>>>> Stashed changes
    }
}
