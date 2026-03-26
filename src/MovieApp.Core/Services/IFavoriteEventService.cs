using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

/// <summary>
/// Coordinates favorite-event workflows for the UI.
/// </summary>
public interface IFavoriteEventService
{
    /// <summary>
    /// Adds an event to the specified user's favorites.
    /// </summary>
    Task AddFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an event from the specified user's favorites.
    /// </summary>
    Task RemoveFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets favorite link rows for the specified user.
    /// </summary>
    Task<IReadOnlyList<FavoriteEvent>> GetFavoritesByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a user has already favorited an event.
    /// </summary>
    Task<bool> ExistsFavoriteAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the actual favorited events for the specified user.
    /// </summary>
    Task<IReadOnlyList<Event>> GetFavoriteEventsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
