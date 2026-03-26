using MovieApp.Core.Models;

namespace MovieApp.Ui.ViewModels;

/// <summary>
/// Represents the top-level shell state shown by <c>MainWindow</c>.
/// </summary>
public sealed class MainViewModel : ViewModelBase
{
    /// <summary>
     /// Creates the shell view model for a successful database-backed startup.
     /// </summary>
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

    /// <summary>
    /// Creates the shell view model shown when application startup fails before
    /// any database-backed feature page can be loaded.
    /// </summary>
    public static MainViewModel CreateStartupError(string message)
    {
        return new MainViewModel("Startup failed", message);
    }

    /// <summary>
    /// Gets the application title displayed in the shell header.
    /// </summary>
    public string AppTitle => "MovieApp";

    /// <summary>
    /// Gets the current database-backed user when startup succeeds.
    /// </summary>
    public User? CurrentUser { get; }

    /// <summary>
    /// Gets the primary shell heading.
    /// </summary>
    public string Greeting { get; }

    /// <summary>
    /// Gets the secondary shell text or startup error details.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the badge text shown in the shell header.
    /// </summary>
    public string UserBadgeText => CurrentUser?.Username[..1].ToUpperInvariant() ?? "?";

    /// <summary>
    /// Gets the shell label describing the active user or startup state.
    /// </summary>
    public string UserLabel => CurrentUser?.Username ?? "Database unavailable";

    /// <summary>
    /// Gets a value indicating whether the configured user was resolved from the database.
    /// </summary>
    public bool UserFoundInDatabase => CurrentUser is not null;

    /// <summary>
    /// Gets the index shown in the user-status combo box.
    /// </summary>
    public int UserDatabaseStateIndex => UserFoundInDatabase ? 0 : 1;
}
