using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace MovieApp.Ui.Views;

/// <summary>
/// Centralizes the reward inventory layout so referral, trivia, jackpot, and checkout
/// reward providers can all feed a single user-facing surface.
/// </summary>
public sealed partial class RewardsPage : Page
{
    private int _rewardBalance;
    public int RewardBalance
    {
        get => _rewardBalance;
        private set
        {
            if (_rewardBalance != value)
            {
                _rewardBalance = value;
                Bindings.Update();
            }
        }
    }

    public RewardsPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            RewardBalance = await App.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);
        }
    }
}
