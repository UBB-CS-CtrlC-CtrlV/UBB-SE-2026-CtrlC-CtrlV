using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    public MainViewModel(User currentUser)
    {
        CurrentUser = currentUser;
        Greeting = currentUser.Username;
        Description = currentUser.StableId;
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

    public string UserBadgeText => CurrentUser?.Username[..1].ToUpperInvariant() ?? "?";

    public string UserLabel => CurrentUser?.Username ?? "Dummy user";

    public bool UserFoundInDatabase => CurrentUser is not null;

    public int UserDatabaseStateIndex => UserFoundInDatabase ? 0 : 1;
}
