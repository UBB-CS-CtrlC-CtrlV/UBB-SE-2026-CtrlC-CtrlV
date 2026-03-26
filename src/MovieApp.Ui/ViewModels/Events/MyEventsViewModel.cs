using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents the user's personal event workspace.
/// </summary>
/// <remarks>
/// The page shell is in place, but the current implementation still returns an
/// empty list until a backing repository flow is wired in.
/// </remarks>
public sealed class MyEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "My Events";

    public ObservableCollection<WatchedEvent> WatchedEvents { get; } = new();

    private WatchedEvent? _selectedWatchedEvent;
    public WatchedEvent? SelectedWatchedEvent
    {
        get => _selectedWatchedEvent;
        set
        {
            if (SetProperty(ref _selectedWatchedEvent, value))
            {
                OnPropertyChanged(nameof(SelectedEventIdText));
                SelectedTargetPrice = value != null ? (double)value.TargetPrice : 0;
            }
        }
    }

    public string SelectedEventIdText => SelectedWatchedEvent?.EventId.ToString() ?? string.Empty;

    private double _selectedTargetPrice;
    public double SelectedTargetPrice
    {
        get => _selectedTargetPrice;
        set => SetProperty(ref _selectedTargetPrice, value);
    }

    private string GetWatchlistFolderPath()
    {
        var folderPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "MovieApp");
        System.IO.Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    public async Task LoadWatchlistAsync()
    {
        var repo = new MovieApp.Infrastructure.LocalPriceWatcherRepository(GetWatchlistFolderPath());
        var items = await repo.GetAllWatchedEventsAsync();
        
        WatchedEvents.Clear();
        foreach (var item in items)
        {
            WatchedEvents.Add(item);
        }
    }

    public async Task SaveSelectedWatchlistAsync()
    {
        if (SelectedWatchedEvent != null)
        {
            SelectedWatchedEvent.TargetPrice = (decimal)SelectedTargetPrice;
            var repo = new MovieApp.Infrastructure.LocalPriceWatcherRepository(GetWatchlistFolderPath());
            
            await repo.RemoveWatchAsync(SelectedWatchedEvent.EventId);
            await repo.AddWatchAsync(SelectedWatchedEvent);
            
            await LoadWatchlistAsync();
        }
    }

    /// <summary>
    /// Loads the events owned by the current user.
    /// </summary>
    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        return Task.FromResult<IReadOnlyList<Event>>([]);
    }
}