using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

public sealed class FavoriteEventService : IFavoriteEventService
{
    private readonly IFavoriteEventRepository _favoriteEventRepository;
    private readonly IEventRepository _eventRepository;

<<<<<<< Updated upstream
    public FavoriteEventService(IFavoriteEventRepository favoriteEventRepository, IEventRepository eventRepository)
=======
    public FavoriteEventService(
        IFavoriteEventRepository favoriteEventRepository,
        IEventRepository eventRepository)
>>>>>>> Stashed changes
    {
        _favoriteEventRepository = favoriteEventRepository;
        _eventRepository = eventRepository;
    }

    public async Task AddFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
<<<<<<< Updated upstream
        if (await ExistsFavoriteAsync(userId, eventId, cancellationToken))
        {
            throw new InvalidOperationException("Event is already favorited by this user.");
        }

        var @event = await _eventRepository.FindByIdAsync(eventId, cancellationToken);
        if (@event == null)
        {
            throw new InvalidOperationException("Event not found.");
=======
        var existingFavorites = await _favoriteEventRepository.FindByUserAsync(userId, cancellationToken);
        if (existingFavorites.Any(f => f.EventId == eventId))
        {
            // Already favorited, ignore or throw
            return;
>>>>>>> Stashed changes
        }

        await _favoriteEventRepository.AddAsync(userId, eventId, cancellationToken);
    }

<<<<<<< Updated upstream
    public async Task RemoveFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        await _favoriteEventRepository.RemoveAsync(userId, eventId, cancellationToken);
    }

    public async Task<IReadOnlyList<FavoriteEvent>> GetFavoritesByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _favoriteEventRepository.FindByUserAsync(userId, cancellationToken);
    }

    public async Task<bool> ExistsFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return await _favoriteEventRepository.ExistsAsync(userId, eventId, cancellationToken);
=======
    public Task RemoveFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return _favoriteEventRepository.RemoveAsync(userId, eventId, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetFavoriteEventsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var favoriteLinks = await _favoriteEventRepository.FindByUserAsync(userId, cancellationToken);
        var events = new List<Event>();

        foreach (var link in favoriteLinks)
        {
            var @event = await _eventRepository.FindByIdAsync(link.EventId, cancellationToken);
            if (@event is not null)
            {
                events.Add(@event);
            }
        }

        return events;
>>>>>>> Stashed changes
    }
}
