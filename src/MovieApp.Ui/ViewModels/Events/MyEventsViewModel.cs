using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents the user's personal event workspace.
/// </summary>
/// <remarks>
/// The page shell is in place, but the current implementation still returns an
/// empty list until a backing repository flow is wired in.
/// </remarks>
public sealed class MyEventsViewModel : EventListPageViewModel
{
    public override string PageTitle => "My Events";

    /// <summary>
    /// Loads the events owned by the current user.
    /// </summary>
    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        return Task.FromResult<IReadOnlyList<Event>>([]);
    }
}
