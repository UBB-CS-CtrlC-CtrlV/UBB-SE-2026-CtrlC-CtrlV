using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the management-oriented CRUD workspace, including the table surface,
/// selected-row preview, and editor mount points.
/// </summary>
public sealed partial class EventManagementPage : Page
{
    private bool _initialized;

    public EventManagementPage()
    {
        ViewModel = new EventManagementViewModel();
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += EventManagementPage_Loaded;
    }

    public EventManagementViewModel ViewModel { get; }

    /// <summary>
    /// Initializes the page once after load so shared event-list behaviors are ready
    /// when the CRUD team wires the live table.
    /// </summary>
    private async void EventManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await InitializeViewModelAsync();
    }

    /// <summary>
    /// Centralizes the initial view-model load path for the page.
    /// </summary>
    private Task InitializeViewModelAsync()
    {
        return ViewModel.InitializeAsync();
    }

    private async void SimulateUpdate_Click(object sender, RoutedEventArgs e)
    {
        var eventRepo = App.EventRepository;
        var notifService = App.NotificationService;
        
        if (eventRepo != null && notifService != null)
        {
            var events = (await eventRepo.GetAllAsync()).ToList();
            var firstEvent = events.FirstOrDefault();
            
            if (firstEvent != null)
            {
                // Drop price
                firstEvent.TicketPrice -= 5;
                if (firstEvent.TicketPrice < 0) firstEvent.TicketPrice = 0;
                
                // Add seats
                firstEvent.CurrentEnrollment = firstEvent.MaxCapacity - 5;
                if (firstEvent.CurrentEnrollment < 0) firstEvent.CurrentEnrollment = 0;
                
                await eventRepo.UpdateAsync(firstEvent);
                
                // Generate notifications
                await notifService.GeneratePriceDropNotificationAsync(firstEvent.Id, firstEvent.Title);
                await notifService.GenerateSeatsAvailableNotificationAsync(firstEvent.Id, firstEvent.Title);
            }
        }
    /// <summary>
    /// Applies the shared event search behavior to the event-management list state.
    /// </summary>
    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel.SetSearchText(searchText);
    }

    /// <summary>
    /// Applies the shared event sort behavior to the event-management list state.
    /// </summary>
    private void SortSelector_SortOptionChanged(object? sender, MovieApp.Core.EventLists.EventSortOption sortOption)
    {
        ViewModel.SetSortOption(sortOption);
    }
}
