using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class MyEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "My Events";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        // TODO: 7 Load only the current user's events when data wiring is added.
        throw new NotImplementedException();
    }
}
