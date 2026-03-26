using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface IUserMovieDiscountRepository
{
    Task AddAsync(Reward reward, CancellationToken cancellationToken = default);

    Task<List<Reward>> GetDiscountsForUserAsync(int userId, CancellationToken cancellationToken = default);
}

