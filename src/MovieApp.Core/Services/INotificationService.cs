using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

public interface INotificationService
{
<<<<<<< Updated upstream
    Task GeneratePriceDropNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default);
    Task GenerateSeatsAvailableNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetNotificationsByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task RemoveNotificationAsync(int notificationId, CancellationToken cancellationToken = default);
=======
    Task<IReadOnlyList<Notification>> GetNotificationsByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task NotifyPriceDropAsync(int eventId, decimal oldPrice, decimal newPrice, CancellationToken cancellationToken = default);

    Task NotifySeatsAvailableAsync(int eventId, int newCapacity, CancellationToken cancellationToken = default);

    Task MarkAsReadOrRemoveAsync(int notificationId, CancellationToken cancellationToken = default);
>>>>>>> Stashed changes
}
