using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.EventLists;
using MovieApp.Core.Models;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

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

    private async void EventManagementPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized) return;
        _initialized = true;
        await ViewModel.InitializeAsync();
    }

    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel.SetSearchText(searchText);
    }

    private void SortSelector_SortOptionChanged(object? sender, EventSortOption sortOption)
    {
        ViewModel.SetSortOption(sortOption);
    }

    private void EventListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedEvent is not Event selected) return;

        ViewModel.FormTitle = selected.Title;
        ViewModel.FormLocation = selected.LocationReference;
        ViewModel.FormEventType = selected.EventType;
        ViewModel.FormDescription = selected.Description ?? string.Empty;
        ViewModel.FormDate = new DateTimeOffset(selected.EventDateTime);
        ViewModel.FormTime = selected.EventDateTime.TimeOfDay;
        ViewModel.FormPrice = (double)selected.TicketPrice;
        ViewModel.FormCapacity = selected.MaxCapacity;
        ViewModel.FormPosterUrl = selected.PosterUrl;
    }

    private async void SimulateUpdate_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SimulateEventUpdateAsync();
    }
}