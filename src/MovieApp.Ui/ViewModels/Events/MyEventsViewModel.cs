using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class MyEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "My Events";

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        return Task.FromResult<IReadOnlyList<Event>>([]);
    }
}
