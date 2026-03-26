using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Tests.Fakes;

public class FakeFavoriteEventRepository : IFavoriteEventRepository
{
    public List<FavoriteEvent> Items { get; } = new();

    public Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        Items.Add(new FavoriteEvent { Id = Items.Count + 1, UserId = userId, EventId = eventId });
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Items.Any(f => f.UserId == userId && f.EventId == eventId));
    }

    public Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<FavoriteEvent>>(Items.Where(f => f.UserId == userId).ToList());
    }

    public Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<int>>(Items.Where(f => f.EventId == eventId).Select(f => f.UserId).ToList());
    }

    public Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<FavoriteEvent>>(Items.Where(f => f.EventId == eventId).ToList());
    }

    public Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(f => f.UserId == userId && f.EventId == eventId);
        if (item != null)
        {
            Items.Remove(item);
        }
        return Task.CompletedTask;
    }
}
