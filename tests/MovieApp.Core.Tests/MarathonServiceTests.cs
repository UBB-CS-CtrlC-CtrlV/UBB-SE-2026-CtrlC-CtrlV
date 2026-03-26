using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;
using Xunit;

namespace MovieApp.Core.Tests;

public sealed class MarathonServiceTests
{
    [Fact]
    public async Task GetWeeklyMarathonsAsync_AssignsWeeklyMarathonsWhenUserHasNone()
    {
        var repository = new StubMarathonRepository();
        var service = CreateService(repository);

        var result = (await service.GetWeeklyMarathonsAsync(userId: 10)).ToList();

        Assert.True(repository.AssignWeeklyMarathonsCalled);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task StartMarathonAsync_ReturnsFalseWhenPrerequisiteIsIncomplete()
    {
        var repository = new StubMarathonRepository
        {
            ActiveMarathons =
            [
                new Marathon
                {
                    Id = 11,
                    Title = "Elite",
                    PrerequisiteMarathonId = 5,
                    IsActive = true,
                },
            ],
            IsPrerequisiteCompletedResult = false,
        };
        var service = CreateService(repository);

        var result = await service.StartMarathonAsync(11);

        Assert.False(result);
        Assert.False(repository.JoinMarathonCalled);
    }

    [Fact]
    public async Task UpdateQuizResultAsync_IncrementsCompletedMoviesAndAveragesAccuracy()
    {
        var progress = new MarathonProgress
        {
            UserId = 10,
            MarathonId = 21,
            TriviaAccuracy = 50,
            CompletedMoviesCount = 1,
        };
        var repository = new StubMarathonRepository
        {
            ProgressByMarathonId = { [21] = progress },
        };
        var service = CreateService(repository);

        await service.UpdateQuizResultAsync(21, correctAnswers: 3);

        Assert.Equal(2, progress.CompletedMoviesCount);
        Assert.Equal(75, progress.TriviaAccuracy);
        Assert.Same(progress, repository.UpdatedProgress);
    }

    [Fact]
    public async Task LogMovieAsync_ReturnsFalseWhenVerificationIsNotPerfect()
    {
        var repository = new StubMarathonRepository
        {
            ProgressByMarathonId =
            {
                [21] = new MarathonProgress
                {
                    UserId = 10,
                    MarathonId = 21,
                    CompletedMoviesCount = 0,
                    TriviaAccuracy = 0,
                },
            },
            MovieCountsByMarathonId = { [21] = 2 },
        };
        var service = CreateService(repository);

        var result = await service.LogMovieAsync(21, movieId: 100, correctAnswers: 2);

        Assert.False(result);
        Assert.Null(repository.UpdatedProgress);
    }

    [Fact]
    public async Task LogMovieAsync_CompletesMarathonWhenFinalMovieIsVerified()
    {
        var progress = new MarathonProgress
        {
            UserId = 10,
            MarathonId = 21,
            CompletedMoviesCount = 1,
            TriviaAccuracy = 100,
        };
        var repository = new StubMarathonRepository
        {
            ProgressByMarathonId = { [21] = progress },
            MovieCountsByMarathonId = { [21] = 2 },
        };
        var service = CreateService(repository);

        var result = await service.LogMovieAsync(21, movieId: 100, correctAnswers: 3);

        Assert.True(result);
        Assert.Equal(2, progress.CompletedMoviesCount);
        Assert.True(progress.IsCompleted);
        Assert.NotNull(progress.FinishedAt);
    }

    private static MarathonService CreateService(StubMarathonRepository repository)
    {
        return new MarathonService(repository, new StubCurrentUserService());
    }

    private sealed class StubCurrentUserService : ICurrentUserService
    {
        public User CurrentUser { get; } = new()
        {
            Id = 10,
            AuthProvider = "seed",
            AuthSubject = "dummy",
            Username = "alice",
        };

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class StubMarathonRepository : IMarathonRepository
    {
        public List<Marathon> ActiveMarathons { get; set; } = [];

        public Dictionary<int, MarathonProgress> ProgressByMarathonId { get; } = [];

        public Dictionary<int, int> MovieCountsByMarathonId { get; } = [];

        public bool AssignWeeklyMarathonsCalled { get; private set; }

        public bool JoinMarathonCalled { get; private set; }

        public bool IsPrerequisiteCompletedResult { get; set; } = true;

        public MarathonProgress? UpdatedProgress { get; private set; }

        public Task<IEnumerable<Marathon>> GetActiveMarathonsAsync()
        {
            return Task.FromResult<IEnumerable<Marathon>>(ActiveMarathons);
        }

        public Task<MarathonProgress?> GetUserProgressAsync(int userId, int marathonId)
        {
            ProgressByMarathonId.TryGetValue(marathonId, out var progress);
            return Task.FromResult(progress);
        }

        public Task<bool> JoinMarathonAsync(int userId, int marathonId)
        {
            JoinMarathonCalled = true;
            return Task.FromResult(true);
        }

        public Task<bool> UpdateProgressAsync(MarathonProgress progress)
        {
            UpdatedProgress = progress;
            ProgressByMarathonId[progress.MarathonId] = progress;
            return Task.FromResult(true);
        }

        public Task<IEnumerable<MarathonProgress>> GetLeaderboardAsync(int marathonId)
        {
            return Task.FromResult<IEnumerable<MarathonProgress>>([]);
        }

        public Task<bool> IsPrerequisiteCompletedAsync(int userId, int prerequisiteMarathonId)
        {
            return Task.FromResult(IsPrerequisiteCompletedResult);
        }

        public Task<int> GetMarathonMovieCountAsync(int marathonId)
        {
            return Task.FromResult(MovieCountsByMarathonId[marathonId]);
        }

        public Task<IEnumerable<Marathon>> GetWeeklyMarathonsForUserAsync(int userId, string weekString)
        {
            IEnumerable<Marathon> result = AssignWeeklyMarathonsCalled
                ? [new Marathon { Id = 1, Title = "Weekly Pick", IsActive = true, WeekScoping = weekString }]
                : [];

            return Task.FromResult(result);
        }

        public Task AssignWeeklyMarathonsAsync(int userId, string weekString, int count = 10)
        {
            AssignWeeklyMarathonsCalled = true;
            return Task.CompletedTask;
        }
    }
}
