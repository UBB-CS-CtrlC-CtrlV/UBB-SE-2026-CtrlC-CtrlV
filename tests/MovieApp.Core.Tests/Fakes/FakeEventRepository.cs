using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Tests.Fakes;

public class FakeEventRepository : IEventRepository
{
    public List<Event> Items { get; } = new();

    public Task<int> AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        Items.Add(@event);
        return Task.FromResult(@event.Id);
    }

    public Task<Event?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Items.FirstOrDefault(e => e.Id == id));
    }

    public Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>(Items);
    }

    public Task<IEnumerable<Event>> GetAllByTypeAsync(string eventType, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Event>>(Items.Where(e => e.EventType == eventType));
    }

    public Task<bool> UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(e => e.Id == @event.Id);
        if (item != null)
        {
            Items.Remove(item);
            Items.Add(@event);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<bool> UpdateEnrollmentAsync(int eventId, int newCount, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(e => e.Id == eventId);
        if (item != null)
        {
            item.CurrentEnrollment = newCount;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task UpdateEventAsync(Event updatedEvent, CancellationToken cancellationToken = default)
    {
        return UpdateAsync(updatedEvent, cancellationToken);
    }

    public Task<bool> DeleteAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var item = Items.FirstOrDefault(e => e.Id == eventId);
        if (item is null) return Task.FromResult(false);
        Items.Remove(item);
        return Task.FromResult(true);
    }
}
