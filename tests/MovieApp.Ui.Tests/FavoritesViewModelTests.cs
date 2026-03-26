using MovieApp.Ui.ViewModels;
using Microsoft.UI.Xaml;
using Xunit;

namespace MovieApp.Ui.Tests;

public sealed class FavoritesViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WithoutFavoriteService_ShowsUnavailableStateAndLoadsNoFavorites()
    {
        var viewModel = new FavoritesViewModel();

        await viewModel.InitializeAsync();

        Assert.False(viewModel.IsServiceAvailable);
        Assert.Equal(Visibility.Visible, viewModel.StatusVisibility);
        Assert.Empty(viewModel.Favorites);
    }
}
