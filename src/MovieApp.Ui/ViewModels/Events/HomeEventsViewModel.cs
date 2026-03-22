using System.ComponentModel;
using MovieApp.Core.Models;
using MovieApp.Core.Repositories;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class HomeEventsViewModel : EventListPageViewModel
{
    private readonly IEventRepository _eventRepository;

    public HomeEventsViewModel(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public override string PageTitle => "Home Events";

    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        var events = await _eventRepository.GetAllAsync();
        return events.ToList();
    }

    private void RebuildSections()
    {
        Sections = BuildSections(VisibleEvents);
    }

    private static IReadOnlyList<EventSection> BuildSections(IEnumerable<Event> events)
    {
        // Group by EventType (trimmed), fallback if empty/whitespace.
        var groups = events
            .GroupBy(e => NormalizeSectionTitle(e.EventType), StringComparer.OrdinalIgnoreCase);

        // Order: all non-fallback sections alphabetical; fallback at end.
        var sections = groups
            .Select(g => new EventSection
            {
                Title = g.Key,
                Events = g.OrderBy(x => x.EventDateTime).ToList(),
            })
            .OrderBy(s => IsFallback(s.Title) ? 1 : 0)
            .ThenBy(s => s.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return sections;
    }

    private static string NormalizeSectionTitle(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return FallbackSectionTitle;
        }

        return eventType.Trim();
    }

    private static bool IsFallback(string title)
        => string.Equals(title, FallbackSectionTitle, StringComparison.OrdinalIgnoreCase);
}
