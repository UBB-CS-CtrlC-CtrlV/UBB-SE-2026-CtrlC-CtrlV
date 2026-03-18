using MovieApp.Models;

namespace MovieApp.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    public MainViewModel(User currentUser)
    {
        CurrentUser = currentUser;
        Greeting = $"{currentUser.Username} is ready to start";
        Description = $"Authenticated as {currentUser.StableId}";
    }

    private MainViewModel(string greeting, string description)
    {
        Greeting = greeting;
        Description = description;
    }

    public static MainViewModel CreateStartupError(string message)
    {
        return new MainViewModel("Startup failed", message);
    }

    public string AppTitle => "MovieApp";

    public User? CurrentUser { get; }

    public string Greeting { get; }

    public string Description { get; }
}
