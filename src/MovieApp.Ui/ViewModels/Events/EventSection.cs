using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class EventSection
{
    public required string Title { get; init; }

    public required IReadOnlyList<Event> Events { get; init; }
}
