using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels;

/// <summary>
/// Provides the favorites screen with the current user's persisted favorite events.
/// </summary>
public sealed partial class FavoritesViewModel : ObservableObject
{
    private readonly IFavoriteEventService? _favoriteEventService;
    private readonly int _currentUserId;

    [ObservableProperty]
    public partial Event? SelectedFavorite { get; set; }

    /// <summary>
    /// Gets the current page-level favorite collection.
    /// </summary>
    public ObservableCollection<Event> Favorites { get; } = new();

    /// <summary>
    /// Gets a value indicating whether the favorites service is available.
    /// </summary>
    public bool IsServiceAvailable => _favoriteEventService is not null && _currentUserId != 0;

    /// <summary>
    /// Gets the visibility of the favorites status message.
    /// </summary>
    public Visibility StatusVisibility => IsServiceAvailable ? Visibility.Collapsed : Visibility.Visible;

    /// <summary>
    /// Gets the status message shown when favorites cannot be loaded.
    /// </summary>
    public string StatusMessage => "Favorites are unavailable because the database connection is not ready.";

    /// <summary>
    /// Gets the command that opens the selected favorite detail workflow.
    /// </summary>
    public ICommand OpenDetailsCommand { get; }

    /// <summary>
    /// Gets the command that removes the selected favorite.
    /// </summary>
    public ICommand RemoveFavoriteCommand { get; }

    /// <summary>
    /// Creates the view model from the application-level favorite-event service.
    /// </summary>
    public FavoritesViewModel()
    {
        _favoriteEventService = App.FavoriteEventService;
        _currentUserId = App.CurrentUserId;

        OpenDetailsCommand = new RelayCommand(OpenDetails, () => SelectedFavorite is not null);
        RemoveFavoriteCommand = new AsyncRelayCommand(RemoveFavoriteAsync, () => SelectedFavorite is not null && IsServiceAvailable);
    }

    partial void OnSelectedFavoriteChanged(Event? value)
    {
        ((RelayCommand)OpenDetailsCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)RemoveFavoriteCommand).NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Loads the current user's favorite events from the database-backed service.
    /// </summary>
    public async Task InitializeAsync()
    {
        Favorites.Clear();

        if (!IsServiceAvailable)
        {
            return;
        }

        var events = await _favoriteEventService!.GetFavoriteEventsByUserIdAsync(_currentUserId);
        foreach (var favoriteEvent in events)
        {
            Favorites.Add(favoriteEvent);
        }
    }

    private void OpenDetails()
    {
        // Navigation to details logic would go here.
    }

    /// <summary>
    /// Removes the selected favorite from the persisted favorites store.
    /// </summary>
    private async Task RemoveFavoriteAsync()
    {
        if (SelectedFavorite is null || !IsServiceAvailable)
        {
            return;
        }

        await _favoriteEventService!.RemoveFavoriteAsync(_currentUserId, SelectedFavorite.Id);
        Favorites.Remove(SelectedFavorite);
        SelectedFavorite = null;
    }
}
