using Microsoft.UI.Xaml.Controls;
using MovieApp.ViewModels.Events;

namespace MovieApp.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        ViewModel = new HomeEventsViewModel();
        InitializeComponent();
        DataContext = ViewModel;

        // TODO: Trigger InitializeViewModelAsync from the page lifecycle once the screen is ready.
    }

    public HomeEventsViewModel ViewModel { get; }

    private Task InitializeViewModelAsync()
    {
        // TODO: Call ViewModel.InitializeAsync() and decide how this page should handle loading and errors.
        throw new NotImplementedException();
    }
}
