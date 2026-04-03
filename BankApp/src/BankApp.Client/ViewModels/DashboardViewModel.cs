// <copyright file="DashboardViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Dashboard;
using BankApp.Core.Entities;
using BankApp.Core.Enums;

namespace BankApp.Client.ViewModels
{
    /// <summary>
    /// Loads and exposes the data required by the dashboard view.
    /// </summary>
    public class DashboardViewModel
    {
        private readonly ApiClient apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
        /// </summary>
        /// <param name="apiClient">The api client.</param>
        public DashboardViewModel(ApiClient apiClient)
        {
            this.apiClient = apiClient;
            this.CurrentUser = null;
            this.State = new ObservableState<DashboardState>(DashboardState.Idle);
            this.Cards = new List<Card>();
            this.RecentTransactions = new List<Transaction>();
            this.RecentTransactionItems = new List<DashboardTransactionItem>();
            this.UnreadNotificationCount = 0;
            this.ErrorMessage = string.Empty;
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        public ObservableState<DashboardState> State { get; private set; }

        /// <summary>
        /// Gets the current user whose dashboard data has been loaded.
        /// </summary>
        public User? CurrentUser { get; private set; }

        /// <summary>
        /// Gets the cards.
        /// </summary>
        public List<Card> Cards { get; private set; }

        /// <summary>
        /// Gets the formatted dashboard transaction rows for display.
        /// </summary>
        public List<DashboardTransactionItem> RecentTransactionItems { get; private set; }

        /// <summary>
        /// Gets the unread notification count.
        /// </summary>
        public int UnreadNotificationCount { get; private set; }

        /// <summary>
        /// Gets the latest load error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the recent transactions.
        /// </summary>
        private List<Transaction> RecentTransactions { get; set; }

        /// <summary>
        /// Fetches dashboard data for the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">A token that can cancel the load operation.</param>
        /// <returns>
        /// <see langword="true"/> if the dashboard data was loaded successfully; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public async Task<bool> LoadDashboard(CancellationToken cancellationToken = default)
        {
            this.State.SetValue(DashboardState.Loading);
            this.ErrorMessage = string.Empty;

            try
            {
                DashboardResponse? response = await this.apiClient.GetAsync<DashboardResponse>(
                    "/api/dashboard/",
                    cancellationToken);

                if (response?.CurrentUser == null)
                {
                    this.ErrorMessage = "The dashboard response was incomplete.";
                    this.State.SetValue(DashboardState.Error);
                    return false;
                }

                this.CurrentUser = response.CurrentUser;
                this.Cards = response.Cards;
                this.RecentTransactions = response.RecentTransactions;
                this.RecentTransactionItems = this.BuildTransactionItems(this.RecentTransactions);
                this.UnreadNotificationCount = response.UnreadNotificationCount;
                this.State.SetValue(DashboardState.Success);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (ApiException ex)
            {
                this.ErrorMessage = ex.StatusCode switch
                {
                    HttpStatusCode.Unauthorized => "Your session expired. Please sign in again.",
                    HttpStatusCode.NotFound => "Dashboard data was not found for this account.",
                    _ => "We couldn't load your dashboard. Please try again.",
                };
                this.State.SetValue(DashboardState.Error);
                this.LogError(nameof(this.LoadDashboard), ex);
                return false;
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "We couldn't load your dashboard. Please try again.";
                this.State.SetValue(DashboardState.Error);
                this.LogError(nameof(this.LoadDashboard), ex);
                return false;
            }
        }

        private List<DashboardTransactionItem> BuildTransactionItems(IEnumerable<Transaction> transactions)
        {
            var items = new List<DashboardTransactionItem>();

            foreach (var transaction in transactions)
            {
                var merchantDisplayName =
                    !string.IsNullOrWhiteSpace(transaction.MerchantName) ? transaction.MerchantName :
                    !string.IsNullOrWhiteSpace(transaction.Description) ? transaction.Description :
                    !string.IsNullOrWhiteSpace(transaction.CounterpartyName) ? transaction.CounterpartyName :
                    "Transaction";

                var sign = string.Equals(transaction.Direction, "Out", StringComparison.OrdinalIgnoreCase)
                    ? "-"
                    : string.Equals(transaction.Direction, "In", StringComparison.OrdinalIgnoreCase) ? "+" : string.Empty;

                items.Add(
                    new DashboardTransactionItem
                {
                    MerchantDisplayName = merchantDisplayName,
                    Type = string.IsNullOrWhiteSpace(transaction.Type) ? "Unknown" : transaction.Type,
                    Currency = string.IsNullOrWhiteSpace(transaction.Currency) ? "N/A" : transaction.Currency,
                    AmountDisplay = $"{sign}{transaction.Amount.ToString("N2", CultureInfo.InvariantCulture)}",
                });
            }

            return items;
        }

        private void LogError(string method, Exception ex) =>
            Console.Error.WriteLine($"[DashboardViewModel] {method}: {ex.Message}");
    }
}
