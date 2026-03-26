using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Coordinates notification creation and retrieval for favorited events.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Generates a price-drop notification using a supplied event title.
    /// </summary>
    Task GeneratePriceDropNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a seats-available notification using a supplied event title.
    /// </summary>
    Task GenerateSeatsAvailableNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications for a user.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a notification.
    /// </summary>
    Task RemoveNotificationAsync(int notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications for a user.
    /// </summary>
    Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates notifications when a price actually decreases.
    /// </summary>
    Task NotifyPriceDropAsync(int eventId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates notifications when seats become available.
    /// </summary>
    Task NotifySeatsAvailableAsync(int eventId, int newCapacity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read or removes it from the active list.
    /// </summary>
    Task MarkAsReadOrRemoveAsync(int notificationId, CancellationToken cancellationToken = default);
}
