using MovieApp.Ui.Views;

namespace MovieApp.Ui.Navigation;

/// <summary>
/// Centralizes the string route tags used by the shell and the page type
/// resolution required to navigate between top-level feature panels.
/// </summary>
public static class AppRouteResolver
{
    public const string Home = "Home";
    public const string MyEvents = "MyEvents";
    public const string EventManagement = "EventManagement";
    public const string Notifications = "Notifications";
    public const string Rewards = "Rewards";
    public const string ReferralArea = "ReferralArea";
    public const string SlotMachine = "SlotMachine";
    public const string TriviaWheel = "TriviaWheel";
    public const string Marathons = "Marathons";

    public const string Slots = "Slots";
    public const string Favorites = "Favorites";

    /// <summary>
    /// Resolves a shell route tag to its corresponding page type.
    /// </summary>
    /// <param name="tag">The route tag emitted by the shell navigation UI.</param>
    /// <returns>The page type that should be loaded into the main frame.</returns>
    public static Type ResolvePageType(string? tag)
    {
        return tag switch
        {
            Home => typeof(HomePage),
            MyEvents => typeof(MyEventsPage),
            Favorites => typeof(FavoritesPage),
            EventManagement => typeof(EventManagementPage),
            Notifications => typeof(NotificationsPage),
            Rewards => typeof(RewardsPage),
            ReferralArea => typeof(ReferralAreaPage),
            SlotMachine => typeof(SlotMachinePage),
            TriviaWheel => typeof(TriviaWheelPage),
            Marathons => typeof(MarathonsPage),
            _ => typeof(HomePage),
        };
    }
}
