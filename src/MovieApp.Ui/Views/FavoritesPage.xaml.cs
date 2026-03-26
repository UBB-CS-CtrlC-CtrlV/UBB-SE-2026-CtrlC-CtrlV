using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the current user's persisted favorite events.
/// </summary>
public sealed partial class FavoritesPage : Page
{
    private bool _initialized;

    public FavoritesViewModel ViewModel { get; }

    public FavoritesPage()
    {
        ViewModel = new FavoritesViewModel();
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += FavoritesPage_Loaded;
    }

    private async void FavoritesPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
    }
}
