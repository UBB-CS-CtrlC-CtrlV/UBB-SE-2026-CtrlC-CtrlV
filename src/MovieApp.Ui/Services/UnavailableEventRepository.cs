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

    private readonly List<Event> _events = new()
    {
        new Event { Id = 1, Title = "Cannes Winner Screening", TicketPrice = 25, MaxCapacity = 100, CurrentEnrollment = 45, EventDateTime = DateTime.Now.AddDays(2), EventType = "Premiere", HistoricalRating = 4.8, LocationReference = "Cinema Hall A", CreatorUserId = 1 },
        new Event { Id = 2, Title = "Vintage Film Marathon", TicketPrice = 40, MaxCapacity = 50, CurrentEnrollment = 10, EventDateTime = DateTime.Now.AddDays(5), EventType = "Marathon", HistoricalRating = 4.5, LocationReference = "Retro Cinema", CreatorUserId = 1 },
        new Event { Id = 3, Title = "Director's Q&A: Sci-Fi Night", TicketPrice = 15, MaxCapacity = 200, CurrentEnrollment = 150, EventDateTime = DateTime.Now.AddDays(7), EventType = "Special", HistoricalRating = 4.9, LocationReference = "Tech Hub Theater", CreatorUserId = 1 }
    };

    public Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>(_events.ToList());
    }

    public Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>(_events.Where(e => e.EventType == eventType).ToList());
    }

    public Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Event?>(_events.FirstOrDefault(e => e.Id == id));
    }

    public Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        _events.Add(@event);
        return Task.FromResult(@event.Id);
    }

    public Task<bool> UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        var existing = _events.FirstOrDefault(e => e.Id == @event.Id);
        if (existing != null)
        {
            _events.Remove(existing);
            _events.Add(@event);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
    {
        var existing = _events.FirstOrDefault(e => e.Id == eventId);
        if (existing != null)
        {
            existing.CurrentEnrollment = newCount;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }
}
