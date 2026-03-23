using Microsoft.UI.Xaml.Controls;

namespace MovieApp.Ui.Views;

/// <summary>
/// Centralizes the reward inventory layout so referral, trivia, jackpot, and checkout
/// reward providers can all feed a single user-facing surface.
/// </summary>
public sealed partial class RewardsPage : Page
{
    public RewardsPage()
    {
        InitializeComponent();
    }
}
