using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.Services;

public class UnavailableFavoriteEventRepository : IFavoriteEventRepository
{
    private readonly List<FavoriteEvent> _items = new();

    public Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        _items.Add(new FavoriteEvent { Id = _items.Count + 1, UserId = userId, EventId = eventId });
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.Any(f => f.UserId == userId && f.EventId == eventId));
    }

    public Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<FavoriteEvent>>(_items.Where(f => f.UserId == userId).ToList());
    }

    public Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<int>>(_items.Where(f => f.EventId == eventId).Select(f => f.UserId).ToList());
    }

    public Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        var item = _items.FirstOrDefault(f => f.UserId == userId && f.EventId == eventId);
        if (item != null)
        {
            _items.Remove(item);
        }
        return Task.CompletedTask;
    }
}
