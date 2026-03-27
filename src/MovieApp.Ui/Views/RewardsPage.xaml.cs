using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MovieApp.Ui.Controls;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

public sealed partial class RewardsPage : Page
{
    private RewardsViewModel? _viewModel;

    private int _rewardBalance;
    private List<SlotRewardItem>? _loadedSlotRewards;
    private bool _slotsListPopulated;

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

        // Load referral reward balance and slot rewards
        if (App.CurrentUserService?.CurrentUser is { } currentUser)
        {
            if (App.AmbassadorRepository is not null)
                RewardBalance = await App.AmbassadorRepository.GetRewardBalanceAsync(currentUser.Id);

            await LoadSlotRewardsAsync(currentUser.Id);
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

    private async Task LoadSlotRewardsAsync(int userId)
    {
        if (App.UserMovieDiscountRepository is null)
            return;

        var rewards = await App.UserMovieDiscountRepository.GetDiscountsForUserAsync(userId);
        _loadedSlotRewards = rewards
            .Select(r => new SlotRewardItem
            {
                RewardId = r.RewardId,
                MovieTitle = r.ApplicabilityScope,
                DiscountText = $"{(int)r.DiscountValue}% off",
                IsRedeemed = r.RedemptionStatus,
            })
            .ToList();

        ApplySlotRewardsToListView();
    }

    private void ApplySlotRewardsToListView()
    {
        if (_loadedSlotRewards is null || SlotsListView is null)
            return;

        SlotsListView.ItemsSource = _loadedSlotRewards;
        _slotsListPopulated = true;
    }

    private void RewardsTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // When the Slots tab is selected for the first time, the ListView
        // may have just been materialized. Apply data if not already done.
        if (!_slotsListPopulated && _loadedSlotRewards is not null &&
            RewardsTabView.SelectedItem == SlotsTab)
        {
            ApplySlotRewardsToListView();
        }
    }

    private void SlotsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SlotsListView.SelectedItem is not SlotRewardItem item)
        {
            ClearDetailPanel();
            return;
        }

        DetailTypeBox.Text = "🎰 Jackpot Discount";
        DetailScopeBox.Text = item.MovieTitle;
        DetailDiscountBox.Text = item.DiscountText;
        DetailStatusBox.Text = item.IsRedeemed ? "Redeemed" : "Available";
        RedeemButton.IsEnabled = !item.IsRedeemed;
    }

    private void ClearDetailPanel()
    {
        DetailTypeBox.Text = "";
        DetailScopeBox.Text = "";
        DetailDiscountBox.Text = "";
        DetailStatusBox.Text = "";
        RedeemButton.IsEnabled = false;
    }

    private async void RedeemButton_Click(object sender, RoutedEventArgs e)
    {
        if (SlotsListView.SelectedItem is not SlotRewardItem item || item.IsRedeemed)
            return;

        if (App.UserMovieDiscountRepository is null || App.CurrentUserService?.CurrentUser is null)
            return;

        await App.UserMovieDiscountRepository.MarkRedeemedAsync(item.RewardId);

        item.IsRedeemed = true;
        DetailStatusBox.Text = "Redeemed";
        RedeemButton.IsEnabled = false;

        // Refresh the list to reflect the status change
        await LoadSlotRewardsAsync(App.CurrentUserService.CurrentUser.Id);

        // Keep the shared discount dictionary current so HomePage badges
        // reflect the redemption without requiring an app restart.
        await EventCard.RefreshDiscountsAsync();
    }

    private async void TriviaRedeemButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null) return;

        TriviaRedeemButton.IsEnabled = false;
        await _viewModel.RedeemTriviaRewardAsync();
        UpdateTriviaRewardUI();
    }
}

/// <summary>
/// Display-only model for slot machine rewards shown in the Rewards page Slots tab.
/// </summary>
public sealed class SlotRewardItem
{
    public int RewardId { get; set; }
    public string MovieTitle { get; set; } = "";
    public string DiscountText { get; set; } = "";
    public bool IsRedeemed { get; set; }
    public string StatusLabel => IsRedeemed ? "Redeemed" : DiscountText;
    public SolidColorBrush StatusBrush => new(
        IsRedeemed
            ? Windows.UI.Color.FromArgb(0x33, 0x94, 0x94, 0x94)
            : Windows.UI.Color.FromArgb(0x33, 0x16, 0xA3, 0x4A));
}