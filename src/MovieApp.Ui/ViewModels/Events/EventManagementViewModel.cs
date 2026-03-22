using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class EventManagementViewModel : EventListPageViewModel
{
    public override string PageTitle => "Event Management";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        return Task.FromResult<IReadOnlyList<Event>>([]);
    }
}
