using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Repositories;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

public sealed partial class HomePage : Page
{
    private bool _initialized;

    public HomePage()
    {
        ViewModel = new HomeEventsViewModel(App.EventRepository!);
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += HomePage_Loaded;
    }

    public HomeEventsViewModel ViewModel { get; }

    private async void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        // Minimal initialization: load demo events then compute group sections.
        await ViewModel.InitializeAsync();
    }
}
