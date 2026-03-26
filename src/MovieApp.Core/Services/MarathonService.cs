using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Core.Services;

/// <summary>
/// Coordinates marathon enrollment, progress, and weekly assignment workflows.
/// </summary>
public sealed class MarathonService : IMarathonService
{
    private readonly IMarathonRepository _marathonRepo;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Creates a service that operates on marathon persistence for the current user.
    /// </summary>
    public MarathonService(
        IMarathonRepository marathonRepo,
        ICurrentUserService currentUserService)
    {
        _marathonRepo = marathonRepo;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the current week's marathons for the specified user, assigning a weekly set when none exist.
    /// </summary>
    public async Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var weekString = $"{now.Year}-W" +
            System.Globalization.ISOWeek.GetWeekOfYear(now).ToString("D2");

        var existing = await _marathonRepo
            .GetWeeklyMarathonsForUserAsync(userId, weekString);

        var list = existing.ToList();

        if (list.Count == 0)
        {
            await _marathonRepo.AssignWeeklyMarathonsAsync(userId, weekString, 10);
            list = (await _marathonRepo
                .GetWeeklyMarathonsForUserAsync(userId, weekString)).ToList();
        }

        return list;
    }

    /// <summary>
    /// Gets the current user's progress for a marathon.
    /// </summary>
    public async Task<MarathonProgress?> GetCurrentProgressAsync(int marathonId)
    {
        var userId = _currentUserService.CurrentUser.Id;
        return await _marathonRepo.GetUserProgressAsync(userId, marathonId);
    }

    /// <summary>
    /// Starts a marathon for the current user if prerequisite rules allow it.
    /// </summary>
    public async Task<bool> StartMarathonAsync(int marathonId)
    {
        var userId = _currentUserService.CurrentUser.Id;

        var existing = await _marathonRepo.GetUserProgressAsync(userId, marathonId);
        if (existing != null) return true;

        var marathons = await _marathonRepo.GetActiveMarathonsAsync();
        var marathon = marathons.FirstOrDefault(m => m.Id == marathonId);

        if (marathon?.PrerequisiteMarathonId is int prereqId)
        {
            var prereqDone = await _marathonRepo
                .IsPrerequisiteCompletedAsync(userId, prereqId);

            if (!prereqDone) return false;
        }

        return await _marathonRepo.JoinMarathonAsync(userId, marathonId);
    }

    /// <summary>
    /// Updates aggregate quiz accuracy after a rapid-fire verification round.
    /// </summary>
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

    /// <summary>
    /// Logs a verified movie when the user answers all three verification questions correctly.
    /// </summary>
    public async Task<bool> LogMovieAsync(int marathonId, int movieId, int correctAnswers)
    {
        if (correctAnswers < 3) return false;

        var userId = _currentUserService.CurrentUser.Id;
        var progress = await _marathonRepo.GetUserProgressAsync(userId, marathonId);
        if (progress is null) return false;

        double newScore = (correctAnswers / 3.0) * 100;
        progress.TriviaAccuracy = progress.CompletedMoviesCount == 0
            ? newScore
            : (progress.TriviaAccuracy + newScore) / 2;

        progress.CompletedMoviesCount++;

        int totalMovies = await _marathonRepo.GetMarathonMovieCountAsync(marathonId);
        if (progress.CompletedMoviesCount >= totalMovies && !progress.IsCompleted)
            progress.FinishedAt = DateTime.UtcNow;

        await _marathonRepo.UpdateProgressAsync(progress);
        return true;
    }
}
