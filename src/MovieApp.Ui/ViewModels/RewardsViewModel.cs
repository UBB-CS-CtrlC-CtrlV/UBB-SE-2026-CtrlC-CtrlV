using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.ViewModels;

/// <summary>
/// Exposes the current user's trivia reward state to the UI.
/// </summary>
public sealed class RewardsViewModel : ViewModelBase
{
    private readonly ITriviaRewardRepository _triviaRewardRepository;
    private readonly int _currentUserId;

    private TriviaReward? _triviaReward;
    private bool _isLoading;

    /// <summary>
    /// Creates the rewards view model for the supplied user.
    /// </summary>
    public RewardsViewModel(ITriviaRewardRepository triviaRewardRepository, int currentUserId)
    {
        _triviaRewardRepository = triviaRewardRepository;
        _currentUserId = currentUserId;
    }

    public TriviaReward? TriviaReward
    {
        get => _triviaReward;
        private set
        {
            _triviaReward = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasTriviaReward));
            OnPropertyChanged(nameof(TriviaRewardStatusText));
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public bool HasTriviaReward => TriviaReward is not null;

    public string TriviaRewardStatusText => TriviaReward is null
        ? "No reward available"
        : TriviaReward.IsRedeemed
            ? "Already redeemed"
            : "Free movie ticket — ready to use!";

    /// <summary>
    /// Loads the current user's unredeemed trivia reward.
    /// </summary>
    public async Task LoadAsync()
    {
        IsLoading = true;
        TriviaReward = await _triviaRewardRepository.GetUnredeemedByUserAsync(_currentUserId);
        IsLoading = false;
    }

    /// <summary>
    /// Redeems the loaded trivia reward and persists the redemption state.
    /// </summary>
    public async Task RedeemTriviaRewardAsync()
    {
        if (TriviaReward is null || TriviaReward.IsRedeemed) return;

        TriviaReward.Redeem();
        await _triviaRewardRepository.MarkAsRedeemedAsync(TriviaReward.Id);
        OnPropertyChanged(nameof(TriviaRewardStatusText));
        OnPropertyChanged(nameof(TriviaReward));
    }
}
