using MovieApp.Core.Models;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class FavoritesViewModel : EventListPageViewModel
{
    private readonly IFavoriteEventService _favoriteEventService;

    public FavoritesViewModel()
    {
        _favoriteEventService = App.FavoriteEventService ?? throw new InvalidOperationException("FavoriteEventService is not initialized.");
    }

    public override string PageTitle => "My Favorites";

    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        var currentUser = App.CurrentUserService?.CurrentUser;
        if (currentUser == null)
        {
            return new List<Event>();
        }

        var favorites = await _favoriteEventService.GetFavoritesByUserAsync(currentUser.Id);
        
        var eventRepository = App.EventRepository 
            ?? throw new InvalidOperationException("EventRepository is not initialized.");
            
        var events = new List<Event>();
        foreach (var favorite in favorites)
        {
            var @event = await eventRepository.FindByIdAsync(favorite.EventId);
            if (@event != null)
            {
                events.Add(@event);
            }
        }

        return events;
    }

    public async Task RemoveFavoriteAsync(int eventId)
    {
        var currentUser = App.CurrentUserService?.CurrentUser;
        if (currentUser == null) return;

        await _favoriteEventService.RemoveFavoriteAsync(currentUser.Id, eventId);
        await InitializeAsync(); // reload events
    }
}
