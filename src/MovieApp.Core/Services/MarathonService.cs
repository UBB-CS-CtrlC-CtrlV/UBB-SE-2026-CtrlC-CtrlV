using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

public sealed class MarathonService : IMarathonService
{
    private readonly IMarathonRepository _marathonRepo;
    private readonly ICurrentUserService _currentUserService;

    public MarathonService(
        IMarathonRepository marathonRepo,
        ICurrentUserService currentUserService)
    {
        _marathonRepo = marathonRepo;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync()
    {
        return await _marathonRepo.GetActiveMarathonsAsync();
    }

    public async Task<MarathonProgress?> GetCurrentProgressAsync(int marathonId)
    {
        var userId = _currentUserService.CurrentUser.Id;
        return await _marathonRepo.GetUserProgressAsync(userId, marathonId);
    }

    public async Task<bool> StartMarathonAsync(int marathonId)
    {
        var userId = _currentUserService.CurrentUser.Id;

        var existing = await _marathonRepo.GetUserProgressAsync(userId, marathonId);
        if (existing != null) return true;

        return await _marathonRepo.JoinMarathonAsync(userId, marathonId);
    }

    public async Task UpdateQuizResultAsync(int marathonId, int correctAnswers)
    {
        var userId = _currentUserService.CurrentUser.Id;
        var progress = await _marathonRepo.GetUserProgressAsync(userId, marathonId);

        if (progress != null)
        {
            double newQuizScore = (correctAnswers / 3.0) * 100;

            progress.TriviaAccuracy = (progress.TriviaAccuracy + newQuizScore) / 2;

            progress.CompletedMoviesCount++;
            await _marathonRepo.UpdateProgressAsync(progress);
        }
    }
}