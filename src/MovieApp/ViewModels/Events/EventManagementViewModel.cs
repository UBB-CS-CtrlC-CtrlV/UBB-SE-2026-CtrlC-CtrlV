using MovieApp.Core.Models;

namespace MovieApp.ViewModels.Events;

public sealed class EventManagementViewModel : EventListPageViewModel
{
    public override string PageTitle => "Event Management";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        // TODO: Load the event-management list or temporarily fall back to BuildSampleEvents().
        throw new NotImplementedException();
    }

    protected override IReadOnlyList<Event> BuildSampleEvents()
    {
        // TODO: Seed representative events that exercise search, sort, and filter behavior on this screen.
        throw new NotImplementedException();
    }
}
