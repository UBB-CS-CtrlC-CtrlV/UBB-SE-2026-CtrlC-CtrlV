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
}
