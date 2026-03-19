using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class HomeEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "Home Events";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        // TODO: 7 Load the home screen event feed when data wiring is added.
        throw new NotImplementedException();
    }
}
