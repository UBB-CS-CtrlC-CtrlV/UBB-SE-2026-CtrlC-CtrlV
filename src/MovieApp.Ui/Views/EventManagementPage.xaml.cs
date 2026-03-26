using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.EventLists;
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
    /// Initializes the page once after load so shared event-list behaviors are ready.
    /// </summary>
    private async void EventManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await ViewModel.InitializeAsync();
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
    private void SortSelector_SortOptionChanged(object? sender, EventSortOption sortOption)
    {
        ViewModel.SetSortOption(sortOption);
    }

    private async void SimulateUpdate_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SimulateEventUpdateAsync();
    }
}
