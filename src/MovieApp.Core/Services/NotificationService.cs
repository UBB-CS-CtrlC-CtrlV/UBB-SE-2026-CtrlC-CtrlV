using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Creates and retrieves notifications related to favorited events.
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IFavoriteEventRepository _favoriteEventRepository;
    private readonly IEventRepository? _eventRepository;

    /// <summary>
    /// Creates the service without event lookups. This supports title-driven notification generation.
    /// </summary>
    public NotificationService(
        INotificationRepository notificationRepository,
        IFavoriteEventRepository favoriteEventRepository)
    {
        _notificationRepository = notificationRepository;
        _favoriteEventRepository = favoriteEventRepository;
    }

    /// <summary>
    /// Creates the service with event lookups enabled.
    /// </summary>
    public NotificationService(
        INotificationRepository notificationRepository,
        IFavoriteEventRepository favoriteEventRepository,
        IEventRepository eventRepository)
        : this(notificationRepository, favoriteEventRepository)
    {
        _eventRepository = eventRepository;
    }

    /// <inheritdoc />
    public Task GeneratePriceDropNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
    {
        return GenerateNotificationForFavoritesAsync(
            eventId,
            "PRICE_DROP",
            $"The ticket price for '{eventTitle}' has dropped!",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task GenerateSeatsAvailableNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
    {
        return GenerateNotificationForFavoritesAsync(
            eventId,
            "SEATS_AVAILABLE",
            $"Seats are now available for '{eventTitle}'!",
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.FindByUserAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveNotificationAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.RemoveAsync(notificationId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.FindByUserAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task NotifyPriceDropAsync(int eventId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default)
    {
        if (newPrice >= oldPrice || _eventRepository is null)
        {
            return;
        }

        var @event = await _eventRepository.FindByIdAsync(eventId, cancellationToken);
        if (@event is null)
        {
            return;
        }

        var favorites = await _favoriteEventRepository.FindByEventAsync(eventId, cancellationToken);
        foreach (var favorite in favorites)
        {
            var notifications = await _notificationRepository.FindByUserAsync(favorite.UserId, cancellationToken);
            if (notifications.Any(n => n.EventId == eventId && n.Type == "PriceDrop" && n.State == NotificationState.Unread))
            {
                continue;
            }

            await _notificationRepository.AddAsync(
                new Notification
                {
                    Id = 0,
                    UserId = favorite.UserId,
                    EventId = eventId,
                    Type = "PriceDrop",
                    Message = $"The price for '{@event.Title}' has dropped to {newPrice:C}!",
                    State = NotificationState.Unread,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task NotifySeatsAvailableAsync(int eventId, int newCapacity, CancellationToken cancellationToken = default)
    {
        if (_eventRepository is null)
        {
            return;
        }

        var @event = await _eventRepository.FindByIdAsync(eventId, cancellationToken);
        if (@event is null || newCapacity <= @event.CurrentEnrollment)
        {
            return;
        }

        var favorites = await _favoriteEventRepository.FindByEventAsync(eventId, cancellationToken);
        foreach (var favorite in favorites)
        {
            var notifications = await _notificationRepository.FindByUserAsync(favorite.UserId, cancellationToken);
            if (notifications.Any(n => n.EventId == eventId && n.Type == "SeatsAvailable" && n.State == NotificationState.Unread))
            {
                continue;
            }

            await _notificationRepository.AddAsync(
                new Notification
                {
                    Id = 0,
                    UserId = favorite.UserId,
                    EventId = eventId,
                    Type = "SeatsAvailable",
                    Message = $"Seats are now available for '{@event.Title}'!",
                    State = NotificationState.Unread,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task MarkAsReadOrRemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        return _notificationRepository.RemoveAsync(notificationId, cancellationToken);
    }

    private async Task GenerateNotificationForFavoritesAsync(int eventId, string type, string message, CancellationToken cancellationToken)
    {
        var favoritedUsers = await _favoriteEventRepository.GetUsersByFavoriteEventAsync(eventId, cancellationToken);

        foreach (var userId in favoritedUsers)
        {
            var notifications = await _notificationRepository.FindByUserAsync(userId, cancellationToken);
            var recentNotification = notifications.FirstOrDefault(notification => notification.EventId == eventId && notification.Type == type);
            if (recentNotification is not null && recentNotification.State == NotificationState.Unread)
            {
                continue;
            }

            await _notificationRepository.AddAsync(
                new Notification
                {
                    Id = 0,
                    UserId = userId,
                    EventId = eventId,
                    Type = type,
                    Message = message,
                    State = NotificationState.Unread,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken);
        }
    }
}
