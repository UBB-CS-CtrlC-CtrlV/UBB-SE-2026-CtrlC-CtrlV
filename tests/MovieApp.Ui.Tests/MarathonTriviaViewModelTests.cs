using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.ViewModels;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class MarathonTriviaViewModelTests
{
    [Fact]
    public async Task StartAsync_BeginsTriviaSessionWithThreeMovieQuestions()
    {
        var viewModel = new MarathonTriviaViewModel(new StubTriviaRepository(BuildMovieQuestions()));

        await viewModel.StartAsync(movieId: 99);

        Assert.True(viewModel.IsPlaying);
        Assert.False(viewModel.IsComplete);
        Assert.Equal("Question 1 of 3", viewModel.ProgressText);
        Assert.NotNull(viewModel.CurrentQuestion);
    }

    [Fact]
    public async Task StartAsync_ThrowsWhenMovieDoesNotHaveEnoughTriviaQuestions()
    {
        var viewModel = new MarathonTriviaViewModel(new StubTriviaRepository(BuildMovieQuestions().Take(2).ToList()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => viewModel.StartAsync(movieId: 99));
    }

    [Fact]
    public async Task SubmitAnswer_CompletesSessionAndMarksPassForPerfectRun()
    {
        var viewModel = new MarathonTriviaViewModel(new StubTriviaRepository(BuildMovieQuestions()));
        await viewModel.StartAsync(movieId: 99);

        viewModel.SubmitAnswer('A');
        viewModel.SubmitAnswer('B');
        viewModel.SubmitAnswer('C');

        Assert.True(viewModel.IsComplete);
        Assert.True(viewModel.IsPassed);
        Assert.Equal(3, viewModel.CorrectCount);
        Assert.Equal("Passed! Movie verified.", viewModel.ResultText);
    }

    [Fact]
    public async Task Reset_ClearsCurrentSessionState()
    {
        var viewModel = new MarathonTriviaViewModel(new StubTriviaRepository(BuildMovieQuestions()));
        await viewModel.StartAsync(movieId: 99);

        viewModel.Reset();

        Assert.False(viewModel.IsPlaying);
        Assert.False(viewModel.IsComplete);
        Assert.Equal(0, viewModel.CorrectCount);
        Assert.Null(viewModel.CurrentQuestion);
        Assert.Equal(string.Empty, viewModel.ProgressText);
    }

    private static IReadOnlyList<TriviaQuestion> BuildMovieQuestions()
    {
        return
        [
            new TriviaQuestion { Id = 1, QuestionText = "Q1", Category = "Movie", OptionA = "A1", OptionB = "B1", OptionC = "C1", OptionD = "D1", CorrectOption = 'A', MovieId = 99 },
            new TriviaQuestion { Id = 2, QuestionText = "Q2", Category = "Movie", OptionA = "A2", OptionB = "B2", OptionC = "C2", OptionD = "D2", CorrectOption = 'B', MovieId = 99 },
            new TriviaQuestion { Id = 3, QuestionText = "Q3", Category = "Movie", OptionA = "A3", OptionB = "B3", OptionC = "C3", OptionD = "D3", CorrectOption = 'C', MovieId = 99 },
        ];
    }

    private sealed class StubTriviaRepository(IReadOnlyList<TriviaQuestion> questions) : ITriviaRepository
    {
        public Task<IEnumerable<TriviaQuestion>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<TriviaQuestion>>([]);
        }

        public Task<IEnumerable<TriviaQuestion>> GetByMovieIdAsync(int movieId, int count = 3, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<TriviaQuestion>>(questions.Where(question => question.MovieId == movieId).Take(count).ToList());
        }
    }
}
