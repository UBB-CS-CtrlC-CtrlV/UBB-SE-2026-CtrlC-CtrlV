using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MovieApp.Core.Repositories;
using MovieApp.Ui.ViewModels.Events;

namespace MovieApp.Ui.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        ViewModel = new HomeEventsViewModel(App.EventRepository!);
        InitializeComponent();
        Loaded += HomePage_Loaded;
    }

    public HomeEventsViewModel ViewModel { get; }

    private async void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.InitializeAsync();
    }
}
