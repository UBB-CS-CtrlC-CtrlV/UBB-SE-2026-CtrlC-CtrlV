using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Repositories;
using MovieApp.Ui.Services;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

/// <summary>
/// Hosts the discovery-first home experience, including horizontal event sections
/// and launch points into the other requirement-driven feature areas.
/// </summary>
public sealed partial class HomePage : Page
{
    private bool _initialized;

    public HomePage()
    {
        ViewModel = new HomeEventsViewModel(GetEventRepository());
        InitializeComponent();
        DataContext = ViewModel;

        Loaded += HomePage_Loaded;
    }

    public HomeEventsViewModel ViewModel { get; }

    /// <summary>
    /// Returns the repository used to populate the home event rows.
    /// </summary>
    private static IEventRepository GetEventRepository()
    {
        return App.EventRepository ?? UnavailableEventRepository.Instance;
    }

    /// <summary>
    /// Initializes the page's event data once after the visual tree is loaded.
    /// </summary>
    private async void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        // Minimal initialization: load demo events then compute group sections.
        await ViewModel.InitializeAsync();
    }
}
