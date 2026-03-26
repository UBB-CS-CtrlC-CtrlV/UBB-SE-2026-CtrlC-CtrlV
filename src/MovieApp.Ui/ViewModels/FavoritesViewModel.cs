using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels;

public sealed partial class FavoritesViewModel : ObservableObject
{
    private readonly IFavoriteEventService _favoriteEventService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial Event? SelectedFavorite { get; set; }

    public ObservableCollection<Event> Favorites { get; } = new();

    public ICommand OpenDetailsCommand { get; }
    public ICommand RemoveFavoriteCommand { get; }

    public FavoritesViewModel()
    {
        _favoriteEventService = App.FavoriteEventService ?? throw new InvalidOperationException("FavoriteEventService is not initialized.");
        _currentUserId = 1;

        OpenDetailsCommand = new RelayCommand(OpenDetails, () => SelectedFavorite is not null);
        RemoveFavoriteCommand = new AsyncRelayCommand(RemoveFavoriteAsync, () => SelectedFavorite is not null);
    }

    partial void OnSelectedFavoriteChanged(Event? value)
    {
        ((RelayCommand)OpenDetailsCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)RemoveFavoriteCommand).NotifyCanExecuteChanged();
    }

    public async Task InitializeAsync()
    {
        if (_currentUserId == 0) return;

        var events = await _favoriteEventService.GetFavoriteEventsByUserIdAsync(_currentUserId);
        Favorites.Clear();
        foreach (var ev in events)
        {
            Favorites.Add(ev);
        }
    }

    private void OpenDetails()
    {
        // Navigation to details logic would go here
    }

    private async Task RemoveFavoriteAsync()
    {
        if (SelectedFavorite is null || _currentUserId == 0) return;

        await _favoriteEventService.RemoveFavoriteAsync(_currentUserId, SelectedFavorite.Id);
        Favorites.Remove(SelectedFavorite);
        SelectedFavorite = null;
    }
}
