using System.ComponentModel;
using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels.Events;

public sealed class HomeEventsViewModel : EventListPageViewModel
{
    private const string FallbackSectionTitle = "Other events";

    private IReadOnlyList<EventSection> _sections = [];

    public HomeEventsViewModel()
    {
        PropertyChanged += OnBasePropertyChanged;
    }

    public override string PageTitle => "Home";

    public IReadOnlyList<EventSection> Sections
    {
        get => _sections;
        private set => SetProperty(ref _sections, value);
    }

    protected override Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        // NOTE: Projectul NU are încă wiring complet pentru events din DB.
        // Pentru a putea valida UI + grouping, folosim o listă demo.
        // Poți înlocui ulterior cu query din dbo.Events.
        IReadOnlyList<Event> demoEvents =
        [
            new Event
            {
                Id = 1,
                Title = "Action Night",
                Description = "Explosions and thrills.",
                EventDateTime = DateTime.Now.AddDays(2),
                LocationReference = "Cinema A",
                TicketPrice = 25m,
                EventType = "Action",
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 2,
                Title = "Horror Marathon",
                Description = "Bring courage.",
                EventDateTime = DateTime.Now.AddDays(5),
                LocationReference = "Cinema B",
                TicketPrice = 30m,
                EventType = "Horror",
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 3,
                Title = "Comedy Evening",
                Description = "Laughs guaranteed.",
                EventDateTime = DateTime.Now.AddDays(1),
                LocationReference = "Cinema A",
                TicketPrice = 18m,
                EventType = "Comedy",
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 4,
                Title = "Mystery Special",
                Description = "Detective vibes.",
                EventDateTime = DateTime.Now.AddDays(3),
                LocationReference = "Cinema C",
                TicketPrice = 22m,
                EventType = "Mystery",
                CreatorUserId = 1,
            },
            new Event
            {
                Id = 5,
                Title = "Uncategorized event",
                Description = "No type set (fallback demo).",
                EventDateTime = DateTime.Now.AddDays(4),
                LocationReference = "Cinema D",
                TicketPrice = 20m,
                EventType = "   ", // fallback
                CreatorUserId = 1,
            },
        ];

        return Task.FromResult(demoEvents);
    }

    private void OnBasePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VisibleEvents))
        {
            RebuildSections();
        }
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
