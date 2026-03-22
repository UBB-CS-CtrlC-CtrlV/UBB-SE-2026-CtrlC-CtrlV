namespace MovieApp.Core.Repositories;

using MovieApp.Core.Models;

public interface IMarathonRepository
{
    Task<IEnumerable<Marathon>> GetActiveMarathonsAsync();

    Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId);

    Task<bool> JoinMarathonAsync(int userId, int marathonId);

    Task<bool> UpdateProgressAsync(MarathonProgress progress);

    Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId);
}