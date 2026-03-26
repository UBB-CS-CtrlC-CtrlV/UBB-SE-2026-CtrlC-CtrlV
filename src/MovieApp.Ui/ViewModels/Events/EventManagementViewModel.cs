using MovieApp.Core.Models;
using MovieApp.Core.Repositories;
using MovieApp.Core.Services;

namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Represents the event-management workspace used for organizer-facing CRUD flows.
/// </summary>
public sealed class EventManagementViewModel : EventListPageViewModel
{
    private readonly IEventRepository? _eventRepository;
    private readonly INotificationService? _notificationService;

    /// <summary>
    /// Creates the management view model using the app-level repositories.
    /// </summary>
    public EventManagementViewModel()
    {
        _eventRepository = App.EventRepository;
        _notificationService = App.NotificationService;
        SimulateUpdateCommand = new MovieApp.Ui.ViewModels.AsyncRelayCommand(SimulateEventUpdateAsync);
        CreateEventCommand = new MovieApp.Ui.ViewModels.AsyncRelayCommand(CreateEventAsync);
        EditEventCommand = new MovieApp.Ui.ViewModels.AsyncRelayCommand(EditEventAsync, () => SelectedEvent is not null);
        DeleteEventCommand = new MovieApp.Ui.ViewModels.AsyncRelayCommand(DeleteEventAsync, () => SelectedEvent is not null);
    }

    public override string PageTitle => "Event Management";

    // ── existing command ──────────────────────────────────────────────────────
    /// <summary>
    /// Gets the command used by the placeholder management UI to simulate notifications.
    /// </summary>
    public System.Windows.Input.ICommand SimulateUpdateCommand { get; }

    // ── new CRUD commands ─────────────────────────────────────────────────────
    public System.Windows.Input.ICommand CreateEventCommand { get; }
    public System.Windows.Input.ICommand EditEventCommand { get; }
    public System.Windows.Input.ICommand DeleteEventCommand { get; }

    // ── selected event ────────────────────────────────────────────────────────
    private Event? _selectedEvent;
    public Event? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            SetProperty(ref _selectedEvent, value);
            ((MovieApp.Ui.ViewModels.AsyncRelayCommand)EditEventCommand).NotifyCanExecuteChanged();
            ((MovieApp.Ui.ViewModels.AsyncRelayCommand)DeleteEventCommand).NotifyCanExecuteChanged();
        }
    }

    // ── form fields ───────────────────────────────────────────────────────────
    public string FormTitle { get; set; } = string.Empty;
    public string FormLocation { get; set; } = string.Empty;
    public string FormEventType { get; set; } = string.Empty;
    public string FormDescription { get; set; } = string.Empty;
    public DateTimeOffset? FormDate { get; set; }
    public TimeSpan? FormTime { get; set; }
    public decimal FormPrice { get; set; }
    public int FormCapacity { get; set; }
    public string FormPosterUrl { get; set; } = string.Empty;

    private string _validationMessage = string.Empty;
    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    // ── loading ───────────────────────────────────────────────────────────────
    /// <inheritdoc />
    protected override async Task<IReadOnlyList<Event>> LoadEventsAsync()
    {
        if (_eventRepository is null)
        {
            return [];
        }

        var events = await _eventRepository.GetAllAsync();
        return events.ToList();
    }

    // ── CRUD operations ───────────────────────────────────────────────────────
    private bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(FormTitle))
        { error = "Title cannot be empty."; return false; }
        if (string.IsNullOrWhiteSpace(FormLocation))
        { error = "Location cannot be empty."; return false; }
        if (FormPrice < 0)
        { error = "Ticket price cannot be negative."; return false; }
        if (FormDate is null)
        { error = "Date is required."; return false; }
        error = string.Empty;
        return true;
    }

    private async Task CreateEventAsync()
    {
        if (_eventRepository is null) return;
        if (!Validate(out var error)) { ValidationMessage = error; return; }

        ValidationMessage = string.Empty;
        var date = FormDate!.Value.Date + (FormTime ?? TimeSpan.Zero);
        var currentUserId = App.CurrentUserService?.CurrentUser.Id ?? 0;

        var newEvent = new Event
        {
            Id = 0,
            Title = FormTitle.Trim(),
            Description = FormDescription,
            LocationReference = FormLocation.Trim(),
            TicketPrice = FormPrice,
            EventDateTime = date,
            EventType = FormEventType.Trim(),
            MaxCapacity = FormCapacity > 0 ? FormCapacity : 50,
            PosterUrl = FormPosterUrl,
            CreatorUserId = currentUserId,
        };

        await _eventRepository.AddAsync(newEvent);
        await InitializeAsync();
    }

    private async Task EditEventAsync()
    {
        if (_eventRepository is null || SelectedEvent is null) return;
        if (!Validate(out var error)) { ValidationMessage = error; return; }

        ValidationMessage = string.Empty;
        var date = FormDate!.Value.Date + (FormTime ?? TimeSpan.Zero);

        var updated = new Event
        {
            Id = SelectedEvent.Id,
            Title = FormTitle.Trim(),
            Description = FormDescription,
            LocationReference = FormLocation.Trim(),
            TicketPrice = FormPrice,
            EventDateTime = date,
            EventType = FormEventType.Trim(),
            MaxCapacity = FormCapacity > 0 ? FormCapacity : SelectedEvent.MaxCapacity,
            PosterUrl = FormPosterUrl,
            CreatorUserId = SelectedEvent.CreatorUserId,
            CurrentEnrollment = SelectedEvent.CurrentEnrollment,
            HistoricalRating = SelectedEvent.HistoricalRating,
        };

        await _eventRepository.UpdateEventAsync(updated);
        await InitializeAsync();
    }

    private async Task DeleteEventAsync()
    {
        if (_eventRepository is null || SelectedEvent is null) return;
        await _eventRepository.DeleteAsync(SelectedEvent.Id);
        SelectedEvent = null;
        await InitializeAsync();
    }

    // ── existing simulation ───────────────────────────────────────────────────
    /// <summary>
    /// Simulates an event update so notification flows can be exercised from the placeholder UI.
    /// </summary>
    public async Task SimulateEventUpdateAsync()
    {
        if (_eventRepository is null || _notificationService is null)
        {
            return;
        }

        var @event = VisibleEvents.FirstOrDefault();
        if (@event is null)
        {
            return;
        }

        var oldPrice = @event.TicketPrice;
        @event.TicketPrice = oldPrice > 5 ? oldPrice - 5 : 0;

        if (@event.CurrentEnrollment > 0)
        {
            @event.CurrentEnrollment -= 1;
        }

        await _eventRepository.UpdateEventAsync(@event);
        await _notificationService.NotifyPriceDropAsync(@event.Id, oldPrice, @event.TicketPrice);
        await _notificationService.NotifySeatsAvailableAsync(@event.Id, @event.MaxCapacity);
    }
}