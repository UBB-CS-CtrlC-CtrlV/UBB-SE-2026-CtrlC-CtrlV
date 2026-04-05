// <copyright file="DashboardViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Dashboard;
using BankApp.Core.Entities;
using BankApp.Core.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Loads and exposes the data required by the dashboard view.
/// </summary>
public class DashboardViewModel
{
    private readonly ApiClient apiClient;
    private readonly ILogger<DashboardViewModel> logger;
    private int currentCardIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for dashboard data requests.</param>
    /// <param name="logger">Logger for dashboard load errors.</param>
    public DashboardViewModel(ApiClient apiClient, ILogger<DashboardViewModel> logger)
    {
        this.apiClient = apiClient;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.CurrentUser = null;
        this.State = new ObservableState<DashboardState>(DashboardState.Idle);
        this.Cards = new List<Card>();
        this.RecentTransactions = new List<Transaction>();
        this.RecentTransactionItems = new List<DashboardTransactionItem>();
        this.UnreadNotificationCount = 0;
        this.ErrorMessage = string.Empty;
        this.currentCardIndex = 0;
    }

    /// <summary>
    /// Gets the state.
    /// </summary>
    public ObservableState<DashboardState> State { get; }

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
    /// Gets the index of the currently displayed card.
    /// </summary>
    public int CurrentCardIndex
    {
        get => this.currentCardIndex;
        private set => this.currentCardIndex = Math.Clamp(value, 0, Math.Max(0, this.Cards.Count - 1));
    }

    /// <summary>
    /// Gets the currently selected card, or <see langword="null"/> if no cards are available.
    /// </summary>
    public Card? SelectedCard => this.Cards.Count > 0 ? this.Cards[this.CurrentCardIndex] : null;

    /// <summary>
    /// Gets a value indicating whether the user can navigate to the previous card.
    /// </summary>
    public bool CanNavigatePrevious => this.Cards.Count > 0 && this.CurrentCardIndex > 0;

    /// <summary>
    /// Gets a value indicating whether the user can navigate to the next card.
    /// </summary>
    public bool CanNavigateNext => this.Cards.Count > 0 && this.CurrentCardIndex < this.Cards.Count - 1;

    /// <summary>
    /// Gets a value indicating whether the user has any linked cards.
    /// </summary>
    public bool HasCards => this.Cards.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the user has any recent transactions.
    /// </summary>
    public bool HasTransactions => this.RecentTransactionItems.Count > 0;

    /// <summary>
    /// Gets the display name of the selected card's brand (falls back to card type when brand is absent).
    /// </summary>
    public string SelectedCardBrandDisplay =>
        this.SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardBrand) ? card.CardType : card.CardBrand
            : string.Empty;

    /// <summary>
    /// Gets the upper-cased cardholder name, or a placeholder when absent.
    /// </summary>
    public string SelectedCardHolderDisplay =>
        this.SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardholderName)
                ? "CARD HOLDER"
                : card.CardholderName.ToUpperInvariant()
            : string.Empty;

    /// <summary>
    /// Gets the formatted expiry date (MM/yy) of the selected card.
    /// </summary>
    public string SelectedCardExpiryDisplay =>
        this.SelectedCard?.ExpiryDate.ToString("MM/yy") ?? string.Empty;

    /// <summary>
    /// Gets the masked card number of the selected card.
    /// </summary>
    public string SelectedCardNumberMasked =>
        this.SelectedCard is { } card ? MaskCardNumber(card.CardNumber) : "**** **** **** ****";

    /// <summary>
    /// Navigates to the previous card if possible.
    /// </summary>
    /// <returns><see langword="true"/> if navigation occurred; otherwise, <see langword="false"/>.</returns>
    public bool NavigatePrevious()
    {
        if (!this.CanNavigatePrevious)
        {
            return false;
        }

        this.CurrentCardIndex--;
        return true;
    }

    /// <summary>
    /// Navigates to the next card if possible.
    /// </summary>
    /// <returns><see langword="true"/> if navigation occurred; otherwise, <see langword="false"/>.</returns>
    public bool NavigateNext()
    {
        if (!this.CanNavigateNext)
        {
            return false;
        }

        this.CurrentCardIndex++;
        return true;
    }

    /// <summary>
    /// Returns a masked representation of a card number, showing only the last four digits.
    /// </summary>
    /// <param name="cardNumber">The raw card number.</param>
    /// <returns>A masked string such as "**** **** **** 1234".</returns>
    public static string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return "**** **** **** ****";
        }

        return cardNumber.Length >= 4
            ? $"**** **** **** {cardNumber[^4..]}"
            : "**** **** **** ****";
    }

    /// <summary>
    /// Builds a human-readable details string for the currently selected card.
    /// The result is suitable for display in a dialog; it contains no UI types.
    /// </summary>
    /// <returns>A multi-line string with card details, or an empty string when no card is selected.</returns>
    public string GetSelectedCardDetails()
    {
        if (this.SelectedCard is not { } card)
        {
            return string.Empty;
        }

        return
            $"Card Type:       {card.CardType}\n" +
            $"Card Brand:      {card.CardBrand ?? "Mastercard"}\n" +
            $"Card Number:     {MaskCardNumber(card.CardNumber)}\n" +
            $"Cardholder:      {card.CardholderName}\n" +
            $"Expiry Date:     {card.ExpiryDate:MM/yy}\n" +
            $"Status:          {card.Status}\n" +
            $"Contactless:     {(card.IsContactlessEnabled ? "Enabled" : "Disabled")}\n" +
            $"Online Payments: {(card.IsOnlineEnabled ? "Enabled" : "Disabled")}";
    }

    /// <summary>
    /// Fetches dashboard data for the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">A token that can cancel the load operation.</param>
    /// <returns>
    /// <see cref="Result.Success"/> if all dashboard data loaded successfully;
    /// otherwise an <see cref="Error"/> describing what went wrong.
    /// </returns>
    public async Task<ErrorOr<Success>> LoadDashboard(CancellationToken cancellationToken = default)
    {
        this.State.SetValue(DashboardState.Loading);
        this.ErrorMessage = string.Empty;

        var result = await this.apiClient.GetAsync<DashboardResponse>(
            "/api/dashboard/",
            cancellationToken);

        return result.Match<ErrorOr<Success>>(
            dashboard =>
            {
                this.CurrentUser = dashboard.CurrentUser;
                this.Cards = dashboard.Cards;
                this.RecentTransactions = dashboard.RecentTransactions;
                this.RecentTransactionItems = this.BuildTransactionItems(this.RecentTransactions);
                this.UnreadNotificationCount = dashboard.UnreadNotificationCount;

                // Reset card navigation to first card after a fresh load.
                this.currentCardIndex = 0;

                this.State.SetValue(DashboardState.Success);
                return Result.Success;
            },
            errors =>
            {
                this.ErrorMessage = errors.First().Type switch
                {
                    ErrorType.Unauthorized => UserMessages.Dashboard.SessionExpired,
                    ErrorType.NotFound => UserMessages.Dashboard.NotFound,
                    _ => UserMessages.Dashboard.LoadFailed,
                };
                this.logger.LogError("LoadDashboard failed: {Errors}", errors);
                this.State.SetValue(DashboardState.Error);
                return errors.First();
            });
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

    /// <summary>
    /// Gets or sets the recent transactions.
    /// </summary>
    private List<Transaction> RecentTransactions { get; set; }
}
