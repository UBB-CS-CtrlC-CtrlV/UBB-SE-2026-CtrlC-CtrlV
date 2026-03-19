using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        ViewModel = new HomeEventsViewModel();
        InitializeComponent();
        DataContext = ViewModel;

        // TODO: Wire page lifecycle initialization here when this screen is connected to real data/loading UX.
    }

    public HomeEventsViewModel ViewModel { get; }

    private Task InitializeViewModelAsync()
    {
        // TODO: Call ViewModel.InitializeAsync() here once page lifecycle, loading, and error handling are implemented.
        throw new NotImplementedException();
    }
}
