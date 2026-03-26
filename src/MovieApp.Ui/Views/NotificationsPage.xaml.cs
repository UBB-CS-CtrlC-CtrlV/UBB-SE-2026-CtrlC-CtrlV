using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the current user's event-related notifications.
/// </summary>
public sealed partial class NotificationsPage : Page
{
    private bool _initialized;

    public NotificationsViewModel ViewModel { get; }

    public NotificationsPage()
    {
        ViewModel = new NotificationsViewModel();
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += NotificationsPage_Loaded;
    }

    private async void NotificationsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
    }
}
