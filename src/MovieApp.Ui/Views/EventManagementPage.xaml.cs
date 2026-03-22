using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        await InitializeViewModelAsync();
    }

    private Task InitializeViewModelAsync()
    {
        return ViewModel.InitializeAsync();
    }
}
