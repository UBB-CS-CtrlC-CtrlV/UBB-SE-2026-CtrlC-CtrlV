using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class TriviaWheelViewModelTests
{
    [Fact]
    public async Task LoadQuestionsAsync_SetsEmptyState_WhenCategoryHasNoQuestions()
    {
        var triviaRepository = new StubTriviaRepository([]);
        var rewardRepository = new StubTriviaRewardRepository();
        var spinRepository = new StubUserSpinRepository();
        var viewModel = new TriviaWheelViewModel(triviaRepository, rewardRepository, spinRepository, 1);

        await viewModel.LoadQuestionsAsync("Actors");

        Assert.True(viewModel.NoQuestionsAvailable);
        Assert.False(viewModel.IsPlaying);
        Assert.Null(viewModel.CurrentQuestion);
        Assert.Equal("No trivia questions are available for this category yet.", viewModel.EmptyStateMessage);
    }

    [Fact]
    public async Task LoadQuestionsAsync_StartsSession_WhenCategoryHasQuestions()
    {
        var triviaRepository = new StubTriviaRepository(
            Enumerable.Range(1, 20)
                .Select(index => new TriviaQuestion
                {
                    Id = index,
                    QuestionText = $"Who directed film {index}?",
                    Category = "Directors",
                    OptionA = "Ridley Scott",
                    OptionB = "Christopher Nolan",
                    OptionC = "Steven Spielberg",
                    OptionD = "James Cameron",
                    CorrectOption = 'B',
                    MovieId = null,
                })
                .ToList());
        var rewardRepository = new StubTriviaRewardRepository();
        var spinRepository = new StubUserSpinRepository();
        var viewModel = new TriviaWheelViewModel(triviaRepository, rewardRepository, spinRepository, 1);

        await viewModel.LoadQuestionsAsync("Directors");

        Assert.False(viewModel.NoQuestionsAvailable);
        Assert.True(viewModel.IsPlaying);
        Assert.NotNull(viewModel.CurrentQuestion);
        Assert.StartsWith("Who directed film ", viewModel.CurrentQuestion!.QuestionText);
        Assert.Equal("1/20", viewModel.ProgressText);
    }

    [Fact]
    public async Task LoadQuestionsAsync_DoesNotStartSessionWhenCategoryHasFewerThanTwentyQuestions()
    {
        var triviaRepository = new StubTriviaRepository(
        [
            new TriviaQuestion
            {
                Id = 1,
                QuestionText = "Question 1",
                Category = "Actors",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = 'A',
                MovieId = null,
            },
            new TriviaQuestion
            {
                Id = 2,
                QuestionText = "Question 2",
                Category = "Actors",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = 'B',
                MovieId = null,
            },
            new TriviaQuestion
            {
                Id = 3,
                QuestionText = "Question 3",
                Category = "Actors",
                OptionA = "A",
                OptionB = "B",
                OptionC = "C",
                OptionD = "D",
                CorrectOption = 'C',
                MovieId = null,
            },
        ]);
        var rewardRepository = new StubTriviaRewardRepository();
        var spinRepository = new StubUserSpinRepository();
        var viewModel = new TriviaWheelViewModel(triviaRepository, rewardRepository, spinRepository, 1);

        await viewModel.LoadQuestionsAsync("Actors");

        Assert.False(viewModel.IsPlaying);
        Assert.Null(viewModel.CurrentQuestion);
    }

    [Fact]
    public async Task InitializeAsync_DisablesTrivia_WhenDatabaseHasNoQuestionData()
    {
        var triviaRepository = new StubTriviaRepository([]);
        var rewardRepository = new StubTriviaRewardRepository();
        var spinRepository = new StubUserSpinRepository();
        var viewModel = new TriviaWheelViewModel(triviaRepository, rewardRepository, spinRepository, 1);

        await viewModel.InitializeAsync();

        Assert.False(viewModel.IsTriviaAvailable);
        Assert.False(viewModel.CanSpin);
        Assert.Equal("Trivia unavailable: no trivia data in the database.", viewModel.AvailabilityMessage);
    }

    [Fact]
    public async Task InitializeAsync_DisablesTrivia_WhenRepositoriesCannotReachDatabase()
    {
        var triviaRepository = new ThrowingTriviaRepository();
        var rewardRepository = new StubTriviaRewardRepository();
        var spinRepository = new ThrowingUserSpinRepository();
        var viewModel = new TriviaWheelViewModel(triviaRepository, rewardRepository, spinRepository, 1);

        await viewModel.InitializeAsync();

        Assert.False(viewModel.IsTriviaAvailable);
        Assert.False(viewModel.CanSpin);
        Assert.Equal("Trivia unavailable: no database connection.", viewModel.AvailabilityMessage);
    }

    private sealed class StubTriviaRepository(IReadOnlyList<TriviaQuestion> questions) : ITriviaRepository
    {
        public Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<TriviaQuestion>>(
                questions.Where(q => string.Equals(q.Category, category, StringComparison.OrdinalIgnoreCase)).ToList());
        }

        public Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieId, int count = 3, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<TriviaQuestion>>([]);
        }
    }

    private sealed class StubTriviaRewardRepository : ITriviaRewardRepository
    {
        public Task AddAsync(TriviaReward reward, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<TriviaReward?> GetUnredeemedByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TriviaReward?>(null);
        }

        public Task MarkAsRedeemedAsync(int rewardId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubUserSpinRepository : IUserSlotMachineStateRepository
    {
        public Task<UserSpinData?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<UserSpinData?>(null);
        }

        public Task CreateAsync(UserSpinData state, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpdateAsync(UserSpinData state, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class ThrowingTriviaRepository : ITriviaRepository
    {
        public Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Database unavailable.");
        }

        public Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieId, int count = 3, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Database unavailable.");
        }
    }

    private sealed class ThrowingUserSpinRepository : IUserSlotMachineStateRepository
    {
        public Task<UserSpinData?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Database unavailable.");
        }

        public Task CreateAsync(UserSpinData state, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Database unavailable.");
        }

        public Task UpdateAsync(UserSpinData state, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Database unavailable.");
        }
    }
}
