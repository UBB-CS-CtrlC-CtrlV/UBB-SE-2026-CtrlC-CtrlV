using MovieApp.Core.Models;

namespace MovieApp.Core.Services;

public interface IMarathonService
{
    Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userId);

    Task<MarathonProgress?> GetCurrentProgressAsync(int marathonId);

    Task<bool> StartMarathonAsync(int marathonId);

    Task UpdateQuizResultAsync(int marathonId, int correctAnswers);
    Task<bool> LogMovieAsync(int marathonId, int movieId, int correctAnswers);
}