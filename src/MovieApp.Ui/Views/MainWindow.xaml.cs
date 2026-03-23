using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the application shell, navigation structure, and the top-level frame
/// where each requirement-driven feature page is loaded.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        AppNavigationView.SelectedItem = HomeNavigationItem;
        NavigateTo("Home");
    }

    public MainViewModel ViewModel { get; }

    private void AppNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string tag)
    {
        var pageType = ResolvePageType(tag);

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    /// <summary>
    /// Resolves a navigation tag to the page that owns that feature area.
    /// </summary>
    private static Type ResolvePageType(string tag)
    {
        return tag switch
        {
            "Home" => typeof(HomePage),
            "DetailsCheckout" => typeof(DetailsCheckoutPage),
            "MyEvents" => typeof(MyEventsPage),
            "EventManagement" => typeof(EventManagementPage),
            "Favorites" => typeof(FavoritesPage),
            "Notifications" => typeof(NotificationsPage),
            "Rewards" => typeof(RewardsPage),
            "ReferralArea" => typeof(ReferralAreaPage),
            "SlotMachine" => typeof(SlotMachinePage),
            "TriviaWheel" => typeof(TriviaWheelPage),
            "Marathons" => typeof(MarathonsPage),
            _ => typeof(HomePage),
        };
    }
}
