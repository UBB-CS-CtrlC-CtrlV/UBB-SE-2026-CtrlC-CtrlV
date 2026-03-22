using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.Services;

/// <summary>
/// Safe fallback event repository used when app startup fails before the real
/// SQL-backed repository can be created.
/// </summary>
public sealed class UnavailableEventRepository : IEventRepository
{
    public static UnavailableEventRepository Instance { get; } = new();

    private UnavailableEventRepository()
    {
    }

    public Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>([]);
    }

    public Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>([]);
    }

    public Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Event?>(null);
    }

    public Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0);
    }

    public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
