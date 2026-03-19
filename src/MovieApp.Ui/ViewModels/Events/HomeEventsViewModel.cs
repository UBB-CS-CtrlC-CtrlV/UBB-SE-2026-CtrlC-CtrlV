using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class HomeEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "Home Events";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        // TODO: Retrieve the raw home-page events here once data wiring exists.
        throw new NotImplementedException();
    }
}
