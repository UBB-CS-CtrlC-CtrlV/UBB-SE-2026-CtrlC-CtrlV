using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

public sealed partial class RewardsPage : Page
{
    private RewardsViewModel? _viewModel;

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

        // Load referral reward balance
        if (App.AmbassadorRepository is not null && App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            RewardBalance = await App.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);
        }

        // Load trivia reward
        if (App.TriviaRewardRepository is not null)
        {
            _viewModel = new RewardsViewModel(App.TriviaRewardRepository, App.CurrentUserId);
            await _viewModel.LoadAsync();
            UpdateTriviaRewardUI();
        }
    }

    private void UpdateTriviaRewardUI()
    {
        if (_viewModel is null) return;

        if (_viewModel.TriviaReward is null)
        {
            // No reward earned yet
            NoRewardBanner.Visibility = Visibility.Visible;
            RewardAvailableBanner.Visibility = Visibility.Collapsed;
            RedeemedBanner.Visibility = Visibility.Collapsed;
        }
        else if (_viewModel.TriviaReward.IsRedeemed)
        {
            // Reward already used
            NoRewardBanner.Visibility = Visibility.Collapsed;
            RewardAvailableBanner.Visibility = Visibility.Collapsed;
            RedeemedBanner.Visibility = Visibility.Visible;
        }
        else
        {
            // Reward ready to use
            NoRewardBanner.Visibility = Visibility.Collapsed;
            RewardAvailableBanner.Visibility = Visibility.Visible;
            RedeemedBanner.Visibility = Visibility.Collapsed;
            RewardEarnedDateText.Text = $"Earned on {_viewModel.TriviaReward.CreatedAt:dd MMM yyyy}";
        }
    }

    private async void RedeemButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null) return;

        RedeemButton.IsEnabled = false;
        await _viewModel.RedeemTriviaRewardAsync();
        UpdateTriviaRewardUI();
    }
}