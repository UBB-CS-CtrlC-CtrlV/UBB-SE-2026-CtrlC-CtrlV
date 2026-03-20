using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default);

    Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default);

    Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default);
}