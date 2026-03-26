using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.ViewModels;

public sealed class TriviaWheelViewModel : ViewModelBase
{
    private readonly ITriviaRepository _triviaRepository;
    private readonly ITriviaRewardRepository _triviaRewardRepository;
    private readonly int _currentUserId;
    private List<TriviaQuestion> _questions = new();

    private string _selectedCategory = string.Empty;
    private bool _canSpin = true;
    private bool _isPlaying;
    private bool _isSessionComplete;
    private TriviaQuestion? _currentQuestion;
    private int _currentQuestionIndex;
    private int _score;
    private bool _hintUsed;

    public TriviaWheelViewModel(
        ITriviaRepository triviaRepository,
        ITriviaRewardRepository triviaRewardRepository,
        int currentUserId)
    {
        _triviaRepository = triviaRepository;
        _triviaRewardRepository = triviaRewardRepository;
        _currentUserId = currentUserId;
    }

    public IReadOnlyList<string> Categories { get; } = new List<string>
    {
        "Actors",
        "Directors",
        "Movie Quotes",
        "Oscars and Awards",
        "General Movie Trivia"
    };

    public bool CanSpin
    {
        get => _canSpin;
        private set
        {
            _canSpin = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RemainingSpinsText));
        }
    }

    public string RemainingSpinsText => CanSpin
        ? "1 spin available today"
        : "Next spin available tomorrow";

    public string SelectedCategory
    {
        get => _selectedCategory;
        private set
        {
            _selectedCategory = value;
            OnPropertyChanged();
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            _isPlaying = value;
            OnPropertyChanged();
        }
    }

    public bool IsSessionComplete
    {
        get => _isSessionComplete;
        private set
        {
            _isSessionComplete = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasEarnedReward));
        }
    }

    public TriviaQuestion? CurrentQuestion
    {
        get => _currentQuestion;
        private set
        {
            _currentQuestion = value;
            OnPropertyChanged();
        }
    }

    public int CurrentQuestionIndex
    {
        get => _currentQuestionIndex;
        private set
        {
            _currentQuestionIndex = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressValue));
            OnPropertyChanged(nameof(ProgressText));
        }
    }

    public int Score
    {
        get => _score;
        private set
        {
            _score = value;
            OnPropertyChanged();
        }
    }

    public bool HintUsed
    {
        get => _hintUsed;
        private set
        {
            _hintUsed = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsHintAvailable));
        }
    }

    public bool IsHintAvailable => !HintUsed;
    public bool HasEarnedReward => Score == 20;
    public double ProgressValue => CurrentQuestionIndex / 20.0 * 100;
    public string ProgressText => $"{CurrentQuestionIndex}/20";

    // ── Methods called by the page code-behind ──────────────────────────────

    /// <summary>
    /// Loads up to 20 randomised questions for the chosen category
    /// and sets the first question as current.
    /// </summary>
    public async Task LoadQuestionsAsync(string category)
    {
        SelectedCategory = category;

        var all = await _triviaRepository.GetByCategoryAsync(category);

        _questions = all
            .OrderBy(_ => Guid.NewGuid())
            .Take(20)
            .ToList();

        CurrentQuestionIndex = 0;
        Score = 0;
        HintUsed = false;
        IsSessionComplete = false;
        IsPlaying = true;
        CanSpin = false;

        AdvanceToNextQuestion();
    }

    /// <summary>
    /// Evaluates the selected answer, updates the score,
    /// and moves to the next question or ends the session.
    /// </summary>
    public void SubmitAnswer(char selectedOption)
    {
        if (CurrentQuestion is null) return;

        if (selectedOption == CurrentQuestion.CorrectOption)
        {
            Score++;
        }

        if (CurrentQuestionIndex >= _questions.Count)
        {
            IsSessionComplete = true;
            IsPlaying = false;

            if (HasEarnedReward)
            {
                _ = GrantRewardAsync();
            }
        }
        else
        {
            AdvanceToNextQuestion();
        }
    }

    /// <summary>
    /// Marks the hint as used — the page handles
    /// which answer options to hide visually.
    /// </summary>
    public void UseHint()
    {
        HintUsed = true;
    }

    /// <summary>
    /// Returns the two incorrect option letters to hide when hint is used.
    /// </summary>
    public IReadOnlyList<char> GetHintOptionsToHide()
    {
        if (CurrentQuestion is null) return Array.Empty<char>();

        var incorrect = new List<char> { 'A', 'B', 'C', 'D' }
            .Where(o => o != CurrentQuestion.CorrectOption)
            .OrderBy(_ => Guid.NewGuid())
            .Take(2)
            .ToList();

        return incorrect;
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private async Task GrantRewardAsync()
    {
        var reward = new TriviaReward
        {
            Id = 0,
            UserId = _currentUserId,
            IsRedeemed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _triviaRewardRepository.AddAsync(reward);
    }

    private void AdvanceToNextQuestion()
    {
        CurrentQuestion = _questions[CurrentQuestionIndex];
        CurrentQuestionIndex++;
    }
}