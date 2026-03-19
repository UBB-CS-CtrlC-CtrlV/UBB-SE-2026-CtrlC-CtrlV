using Microsoft.UI.Xaml.Controls;
using MovieApp.ViewModels.Events;

namespace MovieApp.Views;

public sealed partial class MyEventsPage : Page
{
    public MyEventsPage()
    {
        ViewModel = new MyEventsViewModel();
        InitializeComponent();
        DataContext = ViewModel;

        // TODO: Trigger InitializeViewModelAsync from the page lifecycle once the screen is ready.
    }

    public MyEventsViewModel ViewModel { get; }

    private Task InitializeViewModelAsync()
    {
        // TODO: Call ViewModel.InitializeAsync() and decide how this page should handle loading and errors.
        throw new NotImplementedException();
    }
}
