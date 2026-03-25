using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.ViewModels;

public sealed class MarathonTriviaViewModel : ViewModelBase
{
    private readonly ITriviaRepository _triviaRepository;
    private List<TriviaQuestion> _questions = new();
    private int _currentIndex;
    private int _correctCount;
    private bool _isLoading;
    private bool _isPlaying;
    private bool _isComplete;

    public MarathonTriviaViewModel(ITriviaRepository triviaRepository)
    {
        _triviaRepository = triviaRepository;
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set => SetProperty(ref _isPlaying, value);
    }

    public bool IsComplete
    {
        get => _isComplete;
        private set
        {
            SetProperty(ref _isComplete, value);
            OnPropertyChanged(nameof(IsPassed));
            OnPropertyChanged(nameof(ResultText));
        }
    }

    public int CorrectCount => _correctCount;

    public bool IsPassed => IsComplete && _correctCount == _questions.Count;

    public TriviaQuestion? CurrentQuestion =>
        _currentIndex < _questions.Count ? _questions[_currentIndex] : null;

    public string ProgressText => _questions.Count == 0
        ? string.Empty
        : $"Question {_currentIndex + 1} of {_questions.Count}";

    public string ResultText => !IsComplete
        ? string.Empty
        : IsPassed
            ? "Passed! Movie verified."
            : $"Failed — {_correctCount}/{_questions.Count} correct. Try again.";

    public async Task StartAsync(int movieId)
    {
        IsLoading = true;
        _currentIndex = 0;
        _correctCount = 0;
        IsComplete = false;

        try
        {
            var fetched = await _triviaRepository.GetByMovieIdAsync(movieId, 3);
            _questions = fetched.ToList();

            if (_questions.Count < 3)
                throw new InvalidOperationException(
                    $"Not enough trivia questions for movie {movieId}.");

            IsPlaying = true;
        }
        finally
        {
            IsLoading = false;
        }

        NotifyQuestionChanged();
    }

    public void SubmitAnswer(char selected)
    {
        if (!IsPlaying || CurrentQuestion is null) return;

        if (selected == CurrentQuestion.CorrectOption)
            _correctCount++;

        _currentIndex++;

        if (_currentIndex >= _questions.Count)
        {
            IsPlaying = false;
            IsComplete = true;
        }

        NotifyQuestionChanged();
    }

    public void Reset()
    {
        _questions.Clear();
        _currentIndex = 0;
        _correctCount = 0;
        IsPlaying = false;
        IsComplete = false;
        NotifyQuestionChanged();
    }

    private void NotifyQuestionChanged()
    {
        OnPropertyChanged(nameof(CurrentQuestion));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(ResultText));
    }
}