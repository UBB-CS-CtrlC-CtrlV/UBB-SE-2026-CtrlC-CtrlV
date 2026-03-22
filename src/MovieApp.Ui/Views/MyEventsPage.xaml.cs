using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

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

    private async void MyEventsPage_Loaded(object sender, RoutedEventArgs e)
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
