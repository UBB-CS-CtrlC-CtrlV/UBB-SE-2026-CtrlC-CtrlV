// <copyright file="DashboardViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.Enums;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Loads and exposes the data required by the dashboard view.
/// </summary>
public class DashboardViewModel
{
    private const string CardAtStartErrorCode = "dashboard.card_at_start";
    private const string CardAtStartErrorDescription = "Already at the first card.";
    private const string CardAtEndErrorCode = "dashboard.card_at_end";
    private const string CardAtEndErrorDescription = "Already at the last card.";

    private readonly IApiClient apiClient;
    private readonly ILogger<DashboardViewModel> logger;
    private int currentCardIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="apiClient">The API client used for dashboard data requests.</param>
    /// <param name="logger">Logger for dashboard load errors.</param>
    public DashboardViewModel(IApiClient apiClient, ILogger<DashboardViewModel> logger)
    {
        this.apiClient = apiClient;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.CurrentUser = null;
        this.State = new ObservableState<DashboardState>(DashboardState.Idle);
        this.Cards = new List<CardDto>();
        this.RecentTransactions = new List<TransactionDto>();
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
    public UserSummaryDto? CurrentUser { get; private set; }

    /// <summary>
    /// Gets the cards.
    /// </summary>
    private List<CardDto> Cards { get; set; }

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
    private CardDto? SelectedCard => this.Cards.Count > 0 ? this.Cards[this.CurrentCardIndex] : null;

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
    /// Gets the ordered list of card-dot view models for the carousel indicator.
    /// Each dot knows whether it represents the currently active card.
    /// </summary>
    public IReadOnlyList<CardPageIndicatorViewModel> CardDots =>
        this.Cards.Select((_, index) => new CardPageIndicatorViewModel { IsActive = index == this.CurrentCardIndex })
            .ToList();

    /// <summary>
    /// Gets a value indicating whether the user has any recent transactions.
    /// </summary>
    public bool HasTransactions => this.RecentTransactionItems.Count > 0;

    /// <summary>
    /// Gets the display name of the selected card's brand (falls back to card type when brand is absent).
    /// </summary>
    public string SelectedCardBrandDisplay =>
        this.SelectedCard is { } card
            ? string.IsNullOrWhiteSpace(card.CardBrand) ? card.CardType.ToString() : card.CardBrand
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
    /// <returns>
    /// <see cref="Result.Success"/> if navigation occurred;
    /// otherwise an <see cref="Error"/> when already at the first card.
    /// </returns>
    public ErrorOr<Success> NavigatePrevious()
    {
        if (!this.CanNavigatePrevious)
        {
            return Error.Failure(code: CardAtStartErrorCode, description: CardAtStartErrorDescription);
        }

        this.CurrentCardIndex--;
        return Result.Success;
    }

    /// <summary>
    /// Navigates to the next card if possible.
    /// </summary>
    /// <returns>
    /// <see cref="Result.Success"/> if navigation occurred;
    /// otherwise an <see cref="Error"/> when already at the last card.
    /// </returns>
    public ErrorOr<Success> NavigateNext()
    {
        if (!this.CanNavigateNext)
        {
            return Error.Failure(code: CardAtEndErrorCode, description: CardAtEndErrorDescription);
        }

        this.CurrentCardIndex++;
        return Result.Success;
    }

    /// <summary>
    /// Returns a masked representation of a card number, showing only the last four digits.
    /// </summary>
    /// <param name="cardNumber">The raw card number.</param>
    /// <returns>A masked string such as "**** **** **** 1234".</returns>
    private static string MaskCardNumber(string? cardNumber)
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

        ErrorOr<DashboardResponse> result = await this.apiClient.GetAsync<DashboardResponse>(ApiEndpoints.Dashboard,
            cancellationToken);

        return result.Match<ErrorOr<Success>>(dashboard =>
            {
                if (dashboard.CurrentUser is null)
                {
                    this.ErrorMessage = UserMessages.Dashboard.IncompleteResponse;
                    this.State.SetValue(DashboardState.Error);
                    return Error.Validation(description: UserMessages.Dashboard.IncompleteResponse);
                }

                this.CurrentUser = dashboard.CurrentUser;
                this.Cards = dashboard.Cards;
                this.RecentTransactions = dashboard.RecentTransactions;
                this.RecentTransactionItems = BuildTransactionItems(this.RecentTransactions);
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

    private static List<DashboardTransactionItem> BuildTransactionItems(IEnumerable<TransactionDto> transactions)
    {
        return transactions
            .Select(transaction => new DashboardTransactionItem
            {
                MerchantDisplayName = GetMerchantDisplayName(transaction),
                Currency = GetValueOrFallback(transaction.Currency, "N/A"),
                AmountDisplay = FormatAmountDisplay(transaction)
            })
            .ToList();
    }

    private static string GetMerchantDisplayName(TransactionDto transaction)
    {
        return FirstNonEmpty(transaction.MerchantName,
            transaction.Description,
            transaction.CounterpartyName,
            "Transaction");
    }

    private static string FormatAmountDisplay(TransactionDto transaction)
    {
        string sign = transaction.Direction switch
        {
            TransactionDirection.Out => "-",
            TransactionDirection.In => "+",
            _ => throw new ArgumentOutOfRangeException(nameof(transaction.Direction), transaction.Direction, null)
        };

        return $"{sign}{transaction.Amount.ToString("N2", CultureInfo.InvariantCulture)}";
    }

    private static string GetValueOrFallback(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }

    private List<TransactionDto> RecentTransactions { get; set; }
}
