using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task RemoveAsync(int notificationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken cancellationToken = default);
}
