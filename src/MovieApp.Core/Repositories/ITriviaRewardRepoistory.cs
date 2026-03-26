using MovieApp.Core.Models;

namespace MovieApp.Core.Repositories;

public interface ITriviaRewardRepository
{
    Task AddAsync(TriviaReward reward, CancellationToken cancellationToken = default);

    Task<TriviaReward?> GetUnredeemedByUserAsync(int userId, CancellationToken cancellationToken = default);
}