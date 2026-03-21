using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class HomeEventsViewModel : EventListPageViewModel
{
    private readonly IEventRepository _eventRepository;

    public HomeEventsViewModel(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public override string PageTitle => "Home Events";

    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        var events = await _eventRepository.GetAllAsync();
        return events.ToList();
    }
}
