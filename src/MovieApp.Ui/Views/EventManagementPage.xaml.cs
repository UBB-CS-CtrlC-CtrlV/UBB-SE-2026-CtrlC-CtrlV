using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

public sealed partial class EventManagementPage : Page
{
    public EventManagementPage()
    {
        ViewModel = new EventManagementViewModel();
        InitializeComponent();
        DataContext = ViewModel;

        // TODO: Wire page lifecycle initialization here when this screen is connected to real data/loading UX.
    }

    public EventManagementViewModel ViewModel { get; }

    private Task InitializeViewModelAsync()
    {
        // TODO: Call ViewModel.InitializeAsync() here once page lifecycle, loading, and error handling are implemented.
        throw new NotImplementedException();
    }
}
