using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Displays the event list for a single event section selected from the home page.
/// </summary>
/// <remarks>
/// The section is identified by <see cref="Context.GroupingValue"/>, which is matched
/// against the normalized <see cref="MovieApp.Core.Models.Event.EventType"/> of each event.
/// Events without a valid event type are ignored.
/// </remarks>
public sealed class SectionEventsViewModel(IEventRepository repository, SectionNavigationContext context)
    : EventListPageViewModel
{
    private readonly IEventRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    /// <summary>
    /// Gets the navigation context that defines which section this view model represents.
    /// </summary>
    public SectionNavigationContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Gets the page title shown for the selected section.
    /// </summary>
    public override string PageTitle => Context.Title;

    /// <summary>
    /// Loads all events for the current section and orders them by date.
    /// </summary>
    /// <returns>
    /// A task that resolves to the events whose normalized event type matches
    /// <see cref="SectionNavigationContext.GroupingValue"/>.
    /// </returns>
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        var allEvents = await _repository.GetAllAsync();

        return allEvents
            .Where(e => MatchesSection(e, Context.GroupingValue))
            .OrderBy(e => e.EventDateTime)
            .ToList();
    }

    /// <summary>
    /// Determines whether an event belongs to the currently selected section.
    /// </summary>
    /// <param name="event">The event being evaluated.</param>
    /// <param name="groupingValue">The normalized section grouping value to compare against.</param>
    /// <returns>
    /// <see langword="true"/> when the event has a non-empty event type that matches
    /// the supplied grouping value; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool MatchesSection(Event? @event, string groupingValue)
    {
        if (@event is null || string.IsNullOrWhiteSpace(@event.EventType) || string.IsNullOrWhiteSpace(groupingValue))
        {
            return false;
        }
        var normalizedGroupingValue = groupingValue.Trim();

        var eventGroupingValue = @event.EventType.Trim();

        return string.Equals(
            eventGroupingValue,
            normalizedGroupingValue,
            StringComparison.OrdinalIgnoreCase);
    }
}
