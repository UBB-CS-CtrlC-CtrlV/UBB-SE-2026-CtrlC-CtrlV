using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Implements the favorite-event workflow on top of repository abstractions.
/// </summary>
public sealed class FavoriteEventService : IFavoriteEventService
{
    private readonly IFavoriteEventRepository _favoriteEventRepository;
    private readonly IEventRepository _eventRepository;

    /// <summary>
    /// Creates the service with favorite-link and event repositories.
    /// </summary>
    public FavoriteEventService(
        IFavoriteEventRepository favoriteEventRepository,
        IEventRepository eventRepository)
    {
        _favoriteEventRepository = favoriteEventRepository;
        _eventRepository = eventRepository;
    }

    /// <inheritdoc />
    public async Task AddFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        if (await ExistsFavoriteAsync(userId, eventId, cancellationToken))
        {
            throw new InvalidOperationException("Event is already favorited by this user.");
        }

        var @event = await _eventRepository.FindByIdAsync(eventId, cancellationToken);
        if (@event is null)
        {
            throw new InvalidOperationException("Event not found.");
        }

        await _favoriteEventRepository.AddAsync(userId, eventId, cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return _favoriteEventRepository.RemoveAsync(userId, eventId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<FavoriteEvent>> GetFavoritesByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        return _favoriteEventRepository.FindByUserAsync(userId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> ExistsFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default)
    {
        return _favoriteEventRepository.ExistsAsync(userId, eventId, cancellationToken);
    }

    /// <inheritdoc />
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
    }
}
