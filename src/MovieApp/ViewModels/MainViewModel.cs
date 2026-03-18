using MovieApp.Services;

namespace MovieApp.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    public MainViewModel()
        : this(new GreetingService())
    {
    }

    public MainViewModel(IGreetingService greetingService)
    {
        Greeting = greetingService.GetGreeting();
    }

    public string AppTitle => "MovieApp";

    public string Greeting { get; }

    public string Description => "Shall the development begin";
}
