using MovieApp.Core.Models;

namespace MovieApp.ViewModels.Events;

public sealed class MyEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "My Events";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        // TODO: Load only the current user's events or temporarily fall back to BuildSampleEvents().
        throw new NotImplementedException();
    }

    protected override IReadOnlyList<Event> BuildSampleEvents()
    {
        // TODO: Seed representative user-owned or user-joined events for this screen.
        throw new NotImplementedException();
    }
}
