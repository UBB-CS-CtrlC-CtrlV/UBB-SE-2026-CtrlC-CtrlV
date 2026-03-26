using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.Services;

public class UnavailableNotificationRepository : INotificationRepository
{
    private readonly List<Notification> _items = new();

    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _items.Add(notification);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Notification>>(_items.Where(n => n.UserId == userId).ToList());
    }

    public Task RemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(n => n.Id == notificationId);
        if (item != null)
        {
            _items.Remove(item);
        }
        return Task.CompletedTask;
    }
}
