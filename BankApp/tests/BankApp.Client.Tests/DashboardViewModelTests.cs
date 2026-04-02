using System.Net;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.DTOs.Dashboard;
using BankApp.Core.Entities;
using BankApp.Core.Enums;

namespace BankApp.Client.Tests;

public class DashboardViewModelTests
{
    [Fact]
    public async Task LoadDashboard_WhenResponseIsValid_PopulatesViewModel()
    {
        var apiClient = new FakeApiClient
        {
            Response = new DashboardResponse
            {
                CurrentUser = new User { FullName = "Ada Lovelace" },
                Cards = new List<Card>
                {
                    new() { CardBrand = "Visa", CardType = "Debit", CardholderName = "Ada Lovelace", CardNumber = "1234567812345678" },
                },
                RecentTransactions = new List<Transaction>
                {
                    new() { MerchantName = "Coffee Shop", Type = "Card payment", Direction = "Out", Amount = 12.5m, Currency = "USD" },
                },
                UnreadNotificationCount = 4,
            },
        };

        var viewModel = new DashboardViewModel(apiClient);

        var result = await viewModel.LoadDashboard();

        Assert.True(result);
        Assert.Equal(DashboardState.Success, viewModel.State.Value);
        Assert.Equal("Ada Lovelace", viewModel.CurrentUser?.FullName);
        Assert.Single(viewModel.Cards);
        Assert.Single(viewModel.RecentTransactionItems);
        Assert.Equal("-12.50", viewModel.RecentTransactionItems[0].AmountDisplay);
        Assert.Equal(4, viewModel.UnreadNotificationCount);
        Assert.Equal(string.Empty, viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadDashboard_WhenCurrentUserIsMissing_SetsErrorState()
    {
        var viewModel = new DashboardViewModel(new FakeApiClient
        {
            Response = new DashboardResponse(),
        });

        var result = await viewModel.LoadDashboard();

        Assert.False(result);
        Assert.Equal(DashboardState.Error, viewModel.State.Value);
        Assert.Equal("The dashboard response was incomplete.", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadDashboard_WhenUnauthorized_SetsSessionExpiredMessage()
    {
        var viewModel = new DashboardViewModel(new FakeApiClient
        {
            Exception = new ApiException(HttpStatusCode.Unauthorized, "Unauthorized"),
        });

        var result = await viewModel.LoadDashboard();

        Assert.False(result);
        Assert.Equal(DashboardState.Error, viewModel.State.Value);
        Assert.Equal("Your session expired. Please sign in again.", viewModel.ErrorMessage);
    }

    private sealed class FakeApiClient : ApiClient
    {
        public DashboardResponse? Response { get; init; }

        public Exception? Exception { get; init; }

        public override Task<TResponse?> GetAsync<TResponse>(string endpoint)
        {
            if (this.Exception != null)
            {
                throw this.Exception;
            }

            return Task.FromResult(this.Response as TResponse);
        }
    }
}
