using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.ViewModels;

public sealed class TriviaWheelViewModel : ViewModelBase
{
    
    private const bool DisableDailySpinLimit = false;

    private readonly ITriviaRepository _triviaRepository;
    private readonly ITriviaRewardRepository _triviaRewardRepository;
    private readonly IUserSlotMachineStateRepository _spinRepository;
    private readonly int _currentUserId;
    private List<TriviaQuestion> _questions = new();
    private UserSpinData? _spinData;

    private string _selectedCategory = string.Empty;
    private bool _canSpin = true;
    private bool _isPlaying;
    private bool _isSessionComplete;
    private TriviaQuestion? _currentQuestion;
    private int _currentQuestionIndex;
    private int _score;
    private bool _hintUsed;
    private bool _noQuestionsAvailable;
    private bool _isTriviaAvailable = true;
    private string _availabilityMessage = string.Empty;

    public TriviaWheelViewModel(
        ITriviaRepository triviaRepository,
        ITriviaRewardRepository triviaRewardRepository,
        IUserSlotMachineStateRepository spinRepository,
        int currentUserId)
    {
        _triviaRepository = triviaRepository;
        _triviaRewardRepository = triviaRewardRepository;
        _spinRepository = spinRepository;
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

    /// <summary>
    /// Indicates whether the trivia feature has enough backend data to start a session.
    /// </summary>
    public bool IsTriviaAvailable
    {
        get => _isTriviaAvailable;
        private set
        {
            _isTriviaAvailable = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets a short user-facing explanation when trivia cannot be started.
    /// </summary>
    public string AvailabilityMessage
    {
        get => _availabilityMessage;
        private set
        {
            _availabilityMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates that the selected category currently has no question data available.
    /// </summary>
    public bool NoQuestionsAvailable
    {
        get => _noQuestionsAvailable;
        private set
        {
            _noQuestionsAvailable = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EmptyStateMessage));
        }
    }

    /// <summary>
    /// Gets the user-facing message for the current empty-state condition.
    /// </summary>
    public string EmptyStateMessage => NoQuestionsAvailable
        ? "No trivia questions are available for this category yet."
        : "Spin the wheel to begin!";

    // ── Spin eligibility ─────────────────────────────────────────────────────

    /// <summary>
    /// Loads spin state from DB and sets CanSpin accordingly.
    /// Call this on page load to persist state across tab switches.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            if (DisableDailySpinLimit)
            {
                CanSpin = true;
                await RefreshTriviaAvailabilityAsync();
                return;
            }

            _spinData = await _spinRepository.GetByUserIdAsync(_currentUserId);

            if (_spinData is null)
            {
                // First time user — create a spin record
                _spinData = new UserSpinData
                {
                    UserId = _currentUserId,
                    DailySpinsRemaining = 1,
                    BonusSpins = 0,
                    LoginStreak = 0,
                    EventSpinRewardsToday = 0
                };
                await _spinRepository.CreateAsync(_spinData);
                CanSpin = true;
            }
            else
            {
                // Check if the user has already spun today
                CanSpin = !HasSpunToday(_spinData.LastTriviaSpinReset);
            }

            await RefreshTriviaAvailabilityAsync();
        }
        catch
        {
            CanSpin = false;
            IsTriviaAvailable = false;
            AvailabilityMessage = "Trivia unavailable: no database connection.";
        }
    }

    /// <summary>
    /// Records the spin timestamp in the DB. Call this when the wheel starts spinning.
    /// </summary>
    public async Task RecordSpinAsync()
    {
        if (DisableDailySpinLimit) return;

        _spinData ??= await _spinRepository.GetByUserIdAsync(_currentUserId);

        if (_spinData is null) return;

        _spinData.LastTriviaSpinReset = DateTime.UtcNow;
        await _spinRepository.UpdateAsync(_spinData);
        CanSpin = false;
    }

    // ── Questions ────────────────────────────────────────────────────────────

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
        NoQuestionsAvailable = false;
        CurrentQuestion = null;

        if (_questions.Count == 0)
        {
            IsPlaying = false;
            NoQuestionsAvailable = true;
            return;
        }

        IsPlaying = true;

        AdvanceToNextQuestion();
    }

    /// <summary>
    /// Evaluates the selected answer, updates the score, and advances the session.
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
    /// Marks the one-time hint as consumed for the current session.
    /// </summary>
    public void UseHint()
    {
        HintUsed = true;
    }

    public IReadOnlyList<char> GetHintOptionsToHide()
    {
        if (CurrentQuestion is null) return Array.Empty<char>();

        return new List<char> { 'A', 'B', 'C', 'D' }
            .Where(o => o != CurrentQuestion.CorrectOption)
            .OrderBy(_ => Guid.NewGuid())
            .Take(2)
            .ToList();
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static bool HasSpunToday(DateTime lastSpin)
    {
        if (lastSpin == default) return false;

        // Reset happens at 00:00 server time — compare dates only
        return lastSpin.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Verifies that at least one configured trivia category has questions available.
    /// </summary>
    private async Task RefreshTriviaAvailabilityAsync()
    {
        foreach (var category in Categories)
        {
            var questions = await _triviaRepository.GetByCategoryAsync(category);
            if (questions.Any())
            {
                IsTriviaAvailable = true;
                AvailabilityMessage = string.Empty;
                return;
            }
        }

        IsTriviaAvailable = false;
        AvailabilityMessage = "Trivia unavailable: no trivia data in the database.";
        CanSpin = false;
    }

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
