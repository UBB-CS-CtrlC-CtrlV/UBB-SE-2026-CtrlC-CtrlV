using MovieApp.Ui.Navigation;
using MovieApp.Ui.Views;
using Xunit;

namespace MovieApp.Ui.Tests;

/// <summary>
/// Covers the top-level shell route mapping used by <see cref="MainWindow"/>.
/// </summary>
public sealed class AppRouteResolverTests
{
    [Theory]
    [InlineData(AppRouteResolver.Home, typeof(HomePage))]
    [InlineData(AppRouteResolver.Favorites, typeof(FavoritesPage))]
    [InlineData(AppRouteResolver.MyEvents, typeof(MyEventsPage))]
    [InlineData(AppRouteResolver.Notifications, typeof(NotificationsPage))]
    [InlineData(AppRouteResolver.Rewards, typeof(RewardsPage))]
    [InlineData(AppRouteResolver.ReferralArea, typeof(ReferralAreaPage))]
    [InlineData(AppRouteResolver.SlotMachine, typeof(SlotMachinePage))]
    [InlineData(AppRouteResolver.TriviaWheel, typeof(TriviaWheelPage))]
    [InlineData(AppRouteResolver.Marathons, typeof(MarathonsPage))]
    public void ResolvePageType_KnownRoute_ReturnsExpectedPage(string route, Type expectedPageType)
    {
        var pageType = AppRouteResolver.ResolvePageType(route);

        Assert.Equal(expectedPageType, pageType);
    }

    [Fact]
    public void ResolvePageType_UnknownRoute_FallsBackToHomePage()
    {
        var pageType = AppRouteResolver.ResolvePageType("UnknownRoute");

        Assert.Equal(typeof(HomePage), pageType);
    }
}
