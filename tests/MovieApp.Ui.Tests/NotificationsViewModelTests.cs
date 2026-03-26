using MovieApp.Ui.ViewModels;
using Microsoft.UI.Xaml;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class NotificationsViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WithoutNotificationService_ShowsUnavailableStateAndLoadsNoNotifications()
    {
        var viewModel = new NotificationsViewModel();

        await viewModel.InitializeAsync();

        Assert.False(viewModel.IsServiceAvailable);
        Assert.Equal(Visibility.Visible, viewModel.StatusVisibility);
        Assert.Empty(viewModel.Notifications);
    }
}
