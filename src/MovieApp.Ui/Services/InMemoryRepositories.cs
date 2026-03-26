using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.Services;

public sealed class InMemoryFavoriteEventRepository : IFavoriteEventRepository
{
    private readonly List<FavoriteEvent> _favorites = new();

    public Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<FavoriteEvent>)_favorites.Where(f => f.UserId == userId).ToList());
    }

    public Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<FavoriteEvent>)_favorites.Where(f => f.EventId == eventId).ToList());
    }

    public Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        if (!_favorites.Any(f => f.UserId == userId && f.EventId == eventId))
        {
            _favorites.Add(new FavoriteEvent { Id = _favorites.Count + 1, UserId = userId, EventId = eventId });
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        _favorites.RemoveAll(f => f.UserId == userId && f.EventId == eventId);
        return Task.CompletedTask;
    }
}

public sealed class InMemoryNotificationRepository : INotificationRepository
{
    private readonly List<Notification> _notifications = new();

    public Task<IReadOnlyList<Notification>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<Notification>)_notifications.Where(n => n.UserId == userId).ToList());
    }

    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        notification.Id = _notifications.Count + 1;
        _notifications.Add(notification);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(int notificationId, CancellationToken cancellationToken = default)
    {
        _notifications.RemoveAll(n => n.Id == notificationId);
        return Task.CompletedTask;
    }
}

public sealed class DummyUserRepository : IUserRepository
{
    private readonly User _demoUser = new() { Id = 1, Username = "DemoUser", AuthProvider = "dummy", AuthSubject = "default-user" };

    public Task<User?> FindByAuthIdentityAsync(string provider, string subject, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<User?>(_demoUser);
    }

    public Task<User?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<User?>(_demoUser);
    }
}
