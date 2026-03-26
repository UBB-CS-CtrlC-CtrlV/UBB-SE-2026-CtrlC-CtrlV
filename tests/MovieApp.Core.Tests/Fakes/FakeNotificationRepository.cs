using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Tests.Fakes;

public class FakeNotificationRepository : INotificationRepository
{
    public List<Notification> Items { get; } = new();

    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        Items.Add(notification);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Notification>>(Items.Where(n => n.UserId == userId).ToList());
    }

    public Task RemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(n => n.Id == notificationId);
        if (item != null)
        {
            Items.Remove(item);
        }
        return Task.CompletedTask;
    }
}
