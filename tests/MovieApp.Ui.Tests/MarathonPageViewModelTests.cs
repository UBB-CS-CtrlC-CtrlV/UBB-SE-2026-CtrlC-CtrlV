using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class MarathonPageViewModelTests
{
    [Fact]
    public async Task LoadAsync_PopulatesAvailableMarathons()
    {
        var service = new StubMarathonService
        {
            WeeklyMarathons =
            [
                new Marathon { Id = 1, Title = "One", IsActive = true },
                new Marathon { Id = 2, Title = "Two", IsActive = true },
            ],
        };
        var repository = new StubMarathonRepository();
        var viewModel = new MarathonPageViewModel(service, repository);

        await viewModel.LoadAsync(userId: 10);

        Assert.Equal([1, 2], viewModel.Marathons.Select(m => m.Id));
    }

    [Fact]
    public async Task SelectMarathonAsync_LoadsProgressAndLeaderboard()
    {
        var marathon = new Marathon { Id = 7, Title = "Elite", IsActive = true };
        var progress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 1, TriviaAccuracy = 100 };
        var service = new StubMarathonService { Progress = progress };
        var repository = new StubMarathonRepository
        {
            LeaderboardByMarathonId =
            {
                [7] = [progress],
            },
        };
        var viewModel = new MarathonPageViewModel(service, repository);

        await viewModel.SelectMarathonAsync(marathon);

        Assert.Same(marathon, viewModel.SelectedMarathon);
        Assert.Same(progress, viewModel.CurrentProgress);
        Assert.Equal([10], viewModel.Leaderboard.Select(entry => entry.UserId));
    }

    [Fact]
    public async Task RefreshAfterMovieLoggedAsync_ReloadsProgressAndLeaderboardForSelectedMarathon()
    {
        var marathon = new Marathon { Id = 7, Title = "Elite", IsActive = true };
        var initialProgress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 1, TriviaAccuracy = 100 };
        var refreshedProgress = new MarathonProgress { UserId = 10, MarathonId = 7, CompletedMoviesCount = 2, TriviaAccuracy = 100, FinishedAt = DateTime.UtcNow };
        var service = new StubMarathonService
        {
            ProgressSequence = new Queue<MarathonProgress?>([initialProgress, refreshedProgress]),
        };
        var repository = new StubMarathonRepository
        {
            LeaderboardSequence = new Queue<IReadOnlyList<MarathonProgress>>([[initialProgress], [refreshedProgress]]),
        };
        var viewModel = new MarathonPageViewModel(service, repository);
        await viewModel.SelectMarathonAsync(marathon);

        await viewModel.RefreshAfterMovieLoggedAsync();

        Assert.Same(refreshedProgress, viewModel.CurrentProgress);
        Assert.Equal([2], viewModel.Leaderboard.Select(entry => entry.CompletedMoviesCount));
    }

    [Fact]
    public async Task SelectMarathonAsync_LocksEliteMarathonUntilItsPrerequisiteIsCompleted()
    {
        var marathon = new Marathon
        {
            Id = 7,
            Title = "Elite",
            IsActive = true,
            PrerequisiteMarathonId = 3,
        };
        var service = new StubMarathonService { Progress = null };
        var repository = new StubMarathonRepository
        {
            IsPrerequisiteCompleted = false,
        };
        var viewModel = new MarathonPageViewModel(service, repository);

        await viewModel.SelectMarathonAsync(marathon);

        Assert.True(viewModel.IsLocked);
    }

    private sealed class StubMarathonService : IMarathonService
    {
        public IReadOnlyList<Marathon> WeeklyMarathons { get; set; } = [];

        public MarathonProgress? Progress { get; set; }

        public Queue<MarathonProgress?> ProgressSequence { get; set; } = new();

        public Task<IEnumerable<Marathon>> GetWeeklyMarathonsAsync(int userId)
        {
            return Task.FromResult<IEnumerable<Marathon>>(WeeklyMarathons);
        }

        public Task<MarathonProgress?> GetCurrentProgressAsync(int marathonId)
        {
            if (ProgressSequence.Count > 0)
            {
                return Task.FromResult(ProgressSequence.Dequeue());
            }

            return Task.FromResult(Progress);
        }

        public Task<bool> StartMarathonAsync(int marathonId)
        {
            return Task.FromResult(true);
        }

        public Task UpdateQuizResultAsync(int marathonId, int correctAnswers)
        {
            return Task.CompletedTask;
        }

        public Task<bool> LogMovieAsync(int marathonId, int movieId, int correctAnswers)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class StubMarathonRepository : IMarathonRepository
    {
        public Dictionary<int, IReadOnlyList<MarathonProgress>> LeaderboardByMarathonId { get; } = [];

        public Queue<IReadOnlyList<MarathonProgress>> LeaderboardSequence { get; set; } = new();

        public bool IsPrerequisiteCompleted { get; set; } = true;

        public Task<IEnumerable<Marathon>> GetActiveMarathonsAsync()
        {
            return Task.FromResult<IEnumerable<Marathon>>([]);
        }

        public Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
        {
            return Task.FromResult<MarathonProgress?>(null);
        }

        public Task<bool> JoinMarathonAsync(int userId, int marathonId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> UpdateProgressAsync(MarathonProgress progress)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId)
        {
            if (LeaderboardSequence.Count > 0)
            {
                return Task.FromResult<IEnumerable<MarathonProgress>>(LeaderboardSequence.Dequeue());
            }

            return Task.FromResult<IEnumerable<MarathonProgress>>(
                LeaderboardByMarathonId.TryGetValue(marathonId, out var entries)
                    ? entries
                    : []);
        }

        public Task<bool> IsPrerequisiteCompletedAsync(int userId, int prerequisiteMarathonId)
        {
            return Task.FromResult(IsPrerequisiteCompleted);
        }

        public Task<int> GetMarathonMovieCountAsync(int marathonId)
        {
            return Task.FromResult(0);
        }

        public Task<IEnumerable<Marathon>> GetWeeklyMarathonsForUserAsync(int userId, string weekString)
        {
            return Task.FromResult<IEnumerable<Marathon>>([]);
        }

        public Task AssignWeeklyMarathonsAsync(int userId, string weekString, int count = 10)
        {
            return Task.CompletedTask;
        }
    }
}
