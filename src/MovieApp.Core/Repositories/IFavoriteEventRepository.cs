using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

/// <summary>
/// Provides persistence operations for user favorite-event links.
/// </summary>
public interface IFavoriteEventRepository
{
    /// <summary>
    /// Creates a favorite link between a user and an event.
    /// </summary>
    Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a favorite link between a user and an event.
    /// </summary>
    Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all favorite links owned by a user.
    /// </summary>
    Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a specific favorite link already exists.
    /// </summary>
    Task<bool> ExistsAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the users that favorited a specific event.
    /// </summary>
    Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all favorite links pointing to a specific event.
    /// </summary>
    Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken cancellationToken = default);
}
