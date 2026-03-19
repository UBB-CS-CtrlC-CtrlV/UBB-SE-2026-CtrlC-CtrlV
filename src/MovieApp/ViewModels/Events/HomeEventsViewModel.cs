using MovieApp.Core.Models;

namespace MovieApp.ViewModels.Events;

public sealed class HomeEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "Home Events";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        // TODO: Load the home screen event feed or temporarily fall back to BuildSampleEvents().
        throw new NotImplementedException();
    }

    protected override IReadOnlyList<Event> BuildSampleEvents()
    {
        // TODO: Seed representative home-page events for manual UI verification.
        throw new NotImplementedException();
    }
}
