using MovieApp.Ui.ViewModels.Events;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class PlaceholderEventPagesTests
{
    [Fact]
    public async Task EventManagementViewModel_InitializesToAnEmptySafeState()
    {
        var viewModel = new EventManagementViewModel();

        await viewModel.InitializeAsync();

        Assert.Empty(viewModel.AllEvents);
        Assert.Empty(viewModel.VisibleEvents);
        Assert.True(viewModel.HasNoEvents);
        Assert.False(viewModel.ShowEventList);
    }

    [Fact]
    public async Task MyEventsViewModel_InitializesToAnEmptySafeState()
    {
        var viewModel = new MyEventsViewModel();

        await viewModel.InitializeAsync();

        Assert.Empty(viewModel.AllEvents);
        Assert.Empty(viewModel.VisibleEvents);
        Assert.True(viewModel.HasNoEvents);
        Assert.False(viewModel.ShowEventList);
    }
}
