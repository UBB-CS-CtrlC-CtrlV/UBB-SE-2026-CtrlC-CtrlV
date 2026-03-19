using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

public sealed partial class MyEventsPage : Page
{
    public MyEventsPage()
    {
        ViewModel = new MyEventsViewModel();
        InitializeComponent();
        DataContext = ViewModel;

        // TODO: Wire page lifecycle initialization here when this screen is connected to real data/loading UX.
    }

    public MyEventsViewModel ViewModel { get; }

    private Task InitializeViewModelAsync()
    {
        // TODO: Call ViewModel.InitializeAsync() here once page lifecycle, loading, and error handling are implemented.
        throw new NotImplementedException();
    }
}
