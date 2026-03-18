using Microsoft.UI.Xaml;
using MovieApp.Services;
using MovieApp.ViewModels;

namespace MovieApp.Views;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        ViewModel = new MainViewModel(new GreetingService());
        InitializeComponent();
        Title = ViewModel.AppTitle;
    }

    public MainViewModel ViewModel { get; }
}
