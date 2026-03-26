using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

/// <summary>
/// Owns the personal event workspace for created events, joined events,
/// and the locally persisted price watchlist.
/// </summary>
public sealed partial class MyEventsPage : Page
{
    private bool _initialized;

    public MyEventsPage()
    {
        ViewModel = new MyEventsViewModel();
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += MyEventsPage_Loaded;
    }

    public MyEventsViewModel ViewModel { get; }

    /// <summary>
    /// Initializes the page once after load so bound event-list state is ready
    /// when the implementation team connects live data.
    /// </summary>
    private async void MyEventsPage_Loaded(object sender, RoutedEventArgs e)
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

    /// <summary>
    /// Applies the shared event search behavior to the current personal event-list state.
    /// </summary>
    private void SearchBox_SearchTextChanged(object? sender, string searchText)
    {
        ViewModel.SetSearchText(searchText);
    }
}
