namespace MovieApp.Ui.ViewModels.Events;

/// <summary>
/// Describes a static shortcut action rendered on the home page.
/// </summary>
public sealed class HomeNavigationShortcut
{
    /// <summary>
    /// Gets the user-facing shortcut title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets the explanatory text shown under the shortcut title.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the route tag passed to the application route resolver.
    /// </summary>
    public required string RouteTag { get; init; }
}
