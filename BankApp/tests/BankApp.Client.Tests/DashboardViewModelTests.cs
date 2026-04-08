// <copyright file="DashboardViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.Entities;
using BankApp.Client.Enums;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Client.Tests;

/// <summary>
/// Tests for the <see cref="DashboardViewModel"/>.
/// </summary>
public class DashboardViewModelTests
{
    /// <summary>
    /// When calling LoadDashboard and the response is valid the ViewModel should be populated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenResponseIsValid_PopulatesViewModel()
    {
        var apiClient = new FakeApiClient
        {
            Response = new DashboardResponse
            {
                CurrentUser = new User { FullName = "Ada Lovelace" },
                Cards =
                [
                    new Card
                    {
                        CardBrand = "Visa", CardType = "Debit", CardholderName = "Ada Lovelace",
                        CardNumber = "1234567812345678",
                    },
                ],
                RecentTransactions =
                [
                    new Transaction
                    {
                        MerchantName = "Coffee Shop", Type = "Card payment", Direction = "Out", Amount = 12.5m,
                        Currency = "USD",
                    },
                ],
                UnreadNotificationCount = 4,
            },
        };

        var viewModel = new DashboardViewModel(apiClient, NullLogger<DashboardViewModel>.Instance);

        var result = await viewModel.LoadDashboard();

        Assert.False(result.IsError);
        Assert.Equal(DashboardState.Success, viewModel.State.Value);
        Assert.Equal("Ada Lovelace", viewModel.CurrentUser?.FullName);
        Assert.Single(viewModel.Cards);
        Assert.Single(viewModel.RecentTransactionItems);
        Assert.Equal("-12.50", viewModel.RecentTransactionItems[0].AmountDisplay);
        Assert.Equal(4, viewModel.UnreadNotificationCount);
        Assert.Equal(string.Empty, viewModel.ErrorMessage);
    }

    /// <summary>
    /// When the dashboard response is missing the current user the view model should enter the error state.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenCurrentUserIsMissing_SetsErrorState()
    {
        var viewModel = new DashboardViewModel(
            new FakeApiClient { Response = new DashboardResponse() },
            NullLogger<DashboardViewModel>.Instance);

        var result = await viewModel.LoadDashboard();

        Assert.True(result.IsError);
        Assert.Equal(DashboardState.Error, viewModel.State.Value);
        Assert.Equal("The dashboard response was incomplete.", viewModel.ErrorMessage);
    }

    /// <summary>
    /// When the API returns unauthorized the view model should surface the session-expired error message.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenUnauthorized_SetsSessionExpiredMessage()
    {
        var viewModel = new DashboardViewModel(
            new FakeApiClient { ErrorToReturn = Error.Unauthorized() },
            NullLogger<DashboardViewModel>.Instance);

        var result = await viewModel.LoadDashboard();

        Assert.True(result.IsError);
        Assert.Equal(DashboardState.Error, viewModel.State.Value);
        Assert.Equal("Your session expired. Please sign in again.", viewModel.ErrorMessage);
    }

    /// <summary>
    /// Provides a configurable API client test double for dashboard view model tests.
    /// </summary>
    private sealed class FakeApiClient : ApiClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FakeApiClient"/> class.
        /// </summary>
        public FakeApiClient()
            : base(new ConfigurationBuilder().Build(), NullLogger<ApiClient>.Instance)
        {
        }

        /// <summary>
        /// Gets the response returned by the fake client.
        /// </summary>
        public DashboardResponse? Response { get; init; }

        /// <summary>
        /// Gets the error returned by the fake client, if any.
        /// </summary>
        public Error? ErrorToReturn { get; init; }

        /// <summary>
        /// Returns the configured response or error.
        /// </summary>
        /// <typeparam name="TResponse">The requested response type.</typeparam>
        /// <param name="endpoint">The endpoint requested by the caller.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>A task containing the configured response or error.</returns>
        public override Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            if (this.ErrorToReturn.HasValue)
            {
                return Task.FromResult<ErrorOr<TResponse>>(this.ErrorToReturn.Value);
            }

            return Task.FromResult<ErrorOr<TResponse>>((TResponse)(object)this.Response!);
        }
    }
}
