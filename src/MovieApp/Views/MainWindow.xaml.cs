using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.ViewModels;

namespace MovieApp.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        AppNavigationView.SelectedItem = AppNavigationView.MenuItems[0];
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
        Type pageType = tag switch
        {
            "Home" => typeof(HomePage),
            "MyEvents" => typeof(MyEventsPage),
            "EventManagement" => typeof(EventManagementPage),
            "Favorites" => typeof(FavoritesPage),
            "ReferralArea" => typeof(ReferralAreaPage),
            "SlotMachine" => typeof(SlotMachinePage),
            "TriviaWheel" => typeof(TriviaWheelPage),
            "Marathons" => typeof(MarathonsPage),
            _ => typeof(HomePage),
        };

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }
}