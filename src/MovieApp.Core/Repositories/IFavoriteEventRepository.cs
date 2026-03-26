using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface IFavoriteEventRepository
{
    Task AddAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    Task RemoveAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FavoriteEvent>> FindByUserAsync(int userId, CancellationToken cancellationToken = default);

<<<<<<< Updated upstream
    Task<bool> ExistsAsync(int userId, int eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetUsersByFavoriteEventAsync(int eventId, CancellationToken cancellationToken = default);
=======
    Task<IReadOnlyList<FavoriteEvent>> FindByEventAsync(int eventId, CancellationToken cancellationToken = default);
>>>>>>> Stashed changes
}
