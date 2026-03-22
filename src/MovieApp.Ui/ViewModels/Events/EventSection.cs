using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// A titled group of visible events shown on the home screen.
/// </summary>
public sealed class EventSection
{
    public required string Title { get; init; }

    public required IReadOnlyList<Event> Events { get; init; }
}
