using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents a logical home-page section containing events that share the same grouping value.
/// </summary>
/// <remarks>
/// The home page uses <see cref="Title"/> for display and <see cref="GroupingValue"/>
/// for navigation and filtering.
/// </remarks>
public sealed class EventSection
{
    /// <summary>
    /// Gets the display title shown for the section.
    /// </summary>
    public required string Title { get; init; }
    /// <summary>
    /// Gets the normalized grouping value used to reload this section on the detail page.
    /// </summary>
    public required string GroupingValue { get; init; }
    /// <summary>
    /// Gets the events that belong to this section.
    /// </summary>
    public required IReadOnlyList<Event> Events { get; init; }
}
