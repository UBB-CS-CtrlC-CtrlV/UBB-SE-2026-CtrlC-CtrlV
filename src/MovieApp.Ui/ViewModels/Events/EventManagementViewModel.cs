using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents the event-management workspace used for organizer-facing CRUD flows.
/// </summary>
/// <remarks>
/// The current implementation exposes the shared event-list behavior, but the
/// management data source has not been connected yet.
/// </remarks>
public sealed class EventManagementViewModel : EventListPageViewModel
{
    public override string PageTitle => "Event Management";

    /// <summary>
    /// Loads the management event list.
    /// </summary>
    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        return Task.FromResult<IReadOnlyList<Event>>([]);
    }
}
