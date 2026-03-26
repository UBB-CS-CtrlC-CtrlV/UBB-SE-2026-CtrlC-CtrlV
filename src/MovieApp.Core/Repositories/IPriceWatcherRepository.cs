using System.Collections.Generic;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface IPriceWatcherRepository
{
    Task<List<WatchedEvent>> GetAllWatchedEventsAsync();
    Task<bool> AddWatchAsync(WatchedEvent watchedEvent);
    Task RemoveWatchAsync(int eventId);
    Task<WatchedEvent?> GetWatchAsync(int eventId);
    Task<bool> IsWatchingAsync(int eventId);
}