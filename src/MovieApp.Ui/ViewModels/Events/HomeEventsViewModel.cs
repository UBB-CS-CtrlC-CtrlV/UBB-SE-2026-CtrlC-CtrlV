using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Navigation;
using System.ComponentModel;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Coordinates the event data shown on the home screen, including grouped sections
/// and the navigation metadata required to open a section-specific page.
/// </summary>
/// <remarks>
/// This view model builds its <see cref="Sections"/> collection from the current
/// <see cref="EventListPageViewModel.VisibleEvents"/> set. Events without a valid
/// <see cref="MovieApp.Core.Models.Event.EventType"/> are excluded instead of being
/// placed into a fallback bucket.
/// </remarks>
public sealed class HomeEventsViewModel : EventListPageViewModel
{
    private readonly IEventRepository _repository;
    private IReadOnlyList<EventSection> _sections = [];

    /// <summary>
    /// Creates the home event view model backed by the provided event repository.
    /// </summary>
    /// <param name="repository">Repository used to load the home screen event list.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="repository"/> is <see langword="null"/>.
    /// </exception>
    public HomeEventsViewModel(IEventRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        PropertyChanged += OnBasePropertyChanged;
    }

    /// <summary>
    /// Gets the title displayed for the home page.
    /// </summary>
    public override string PageTitle => "Home";

    /// <summary>
    /// Gets the static shortcut links rendered on the home page.
    /// </summary>
    /// <remarks>
    /// Each shortcut maps a display title and description to an application route tag.
    /// </remarks>
    public IReadOnlyList<HomeNavigationShortcut> NavigationShortcuts { get; } =
    [
        new HomeNavigationShortcut
        {
            Title = "My Events",
            Description = "Open your personal event workspace.",
            RouteTag = AppRouteResolver.MyEvents,
        },
        new HomeNavigationShortcut
        {
            Title = "Event Management",
            Description = "Open the event administration workspace.",
            RouteTag = AppRouteResolver.EventManagement,
        },
    ];

    /// <summary>
    /// Gets the event sections currently visible on the home page.
    /// </summary>
    /// <remarks>
    /// The collection is rebuilt from <see cref="EventListPageViewModel.VisibleEvents"/>
    /// whenever the visible event list changes.
    /// </remarks>
    public IReadOnlyList<EventSection> Sections
    {
        get => _sections;
        private set => SetProperty(ref _sections, value);
    }

    /// <summary>
    /// Creates the navigation payload used to open a section-specific event page.
    /// </summary>
    /// <param name="section">The section selected by the user.</param>
    /// <returns>
    /// A navigation context containing the section title and normalized grouping value.
    /// </returns>
    /// <remarks>
    /// Use this when navigating from the home page into <c>SectionEventsPage</c> so
    /// the destination page can reload only the events belonging to the chosen section.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="section"/> is <see langword="null"/>.
    /// </exception>
    public SectionNavigationContext CreateNavigationContext(EventSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        return new SectionNavigationContext
        {
            Title = section.Title,
            GroupingValue = section.GroupingValue,
        };
    }

    /// <summary>
    /// Loads the raw event set used by the home page.
    /// </summary>
    /// <returns>
    /// A task that resolves to all events returned by the configured repository.
    /// </returns>
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        var allEvents = await _repository.GetAllAsync();
        return allEvents.ToList();
    }

    private void OnBasePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VisibleEvents))
        {
            Sections = BuildSections(VisibleEvents);
        }
    }

    /// <summary>
    /// Groups the already-transformed visible event sequence into home-page sections.
    /// </summary>
    /// <remarks>
    /// This method preserves the incoming event order inside each section so the
    /// active list sort mode remains consistent on the home screen.
    /// </remarks>
    private static IReadOnlyList<EventSection> BuildSections(IEnumerable<Event> events)
    {
        var sectionsByType = new Dictionary<string, EventSection>(StringComparer.OrdinalIgnoreCase);
        var orderedSections = new List<EventSection>();

        foreach (var @event in events.Where(e => !string.IsNullOrWhiteSpace(e.EventType)))
        {
            var eventType = @event.EventType.Trim();
            if (!sectionsByType.TryGetValue(eventType, out var section))
            {
                section = new EventSection
                {
                    Title = eventType,
                    GroupingValue = eventType,
                    Events = new List<Event>(),
                };
                sectionsByType[eventType] = section;
                orderedSections.Add(section);
            }

            ((List<Event>)section.Events).Add(@event);
        }

        return orderedSections;
    }
}
