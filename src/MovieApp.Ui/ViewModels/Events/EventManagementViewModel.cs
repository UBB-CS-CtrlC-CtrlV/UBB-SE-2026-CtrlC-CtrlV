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
    }

    public override string PageTitle => "Event Management";

    /// <summary>
    /// Gets the command used by the placeholder management UI to simulate notifications.
    /// </summary>
    public System.Windows.Input.ICommand SimulateUpdateCommand { get; }

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
