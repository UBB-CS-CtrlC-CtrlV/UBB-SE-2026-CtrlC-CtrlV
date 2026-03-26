using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels;

using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

/// <summary>
/// Provides the saved-event inventory surface while keeping favorite-event alerts
/// separate in their own page.
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
<<<<<<< Updated upstream
        Loaded += async (s, e) => { await ViewModel.InitializeAsync(); };
    }

    public FavoritesViewModel ViewModel { get; }

    private async void RemoveFavorite_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is MovieApp.Core.Models.Event @event)
        {
            await ViewModel.RemoveFavoriteAsync(@event.Id);
        }
=======
        Loaded += FavoritesPage_Loaded;
    }

    private async void FavoritesPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;
        _initialized = true;
        await ViewModel.InitializeAsync();
>>>>>>> Stashed changes
    }
}
