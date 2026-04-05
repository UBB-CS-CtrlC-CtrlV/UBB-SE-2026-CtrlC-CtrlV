// <copyright file="DashboardView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace BankApp.Client.Views;

/// <summary>
/// Displays the authenticated user's account summary, card carousel, and recent transactions.
/// </summary>
public sealed partial class DashboardView : IStateObserver<DashboardState>
{
    private readonly DashboardViewModel viewModel;
    private int currentCardIndex;
    private bool isObserverAttached;
    private CancellationTokenSource? loadCancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardView"/> class.
    /// </summary>
    /// <param name="viewModel">The view model that loads account data and exposes dashboard state.</param>
    public DashboardView(DashboardViewModel viewModel)
    {
        this.InitializeComponent();

        this.viewModel = viewModel;
    }

    /// <inheritdoc/>
    public void Update(DashboardState state)
    {
        this.OnStateChanged(state);
    }

    /// <inheritdoc/>
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        this.AttachObserver();
        _ = this.RunUiTaskAsync(this.LoadDashboardAsync);
    }

    /// <inheritdoc/>
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        this.CancelPendingLoad();
        this.DetachObserver();
    }

    /// <summary>
    /// Reacts to dashboard state updates from the view model.
    /// </summary>
    /// <param name="state">The new state.</param>
    private void OnStateChanged(DashboardState state)
    {
        this.DispatcherQueue.TryEnqueue(() =>
        {
            switch (state)
            {
                case DashboardState.Loading:
                    this.ShowLoading();
                    break;

                case DashboardState.Success:
                    this.HideLoading();
                    this.ErrorInfoBar.IsOpen = false;
                    this.RefreshUi();
                    break;

                case DashboardState.Error:
                    this.HideLoading();
                    this.ShowError(this.viewModel.ErrorMessage);
                    break;
                case DashboardState.Idle:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        });
    }

    private async Task LoadDashboardAsync()
    {
        this.CancelPendingLoad();
        this.loadCancellationTokenSource = new CancellationTokenSource();

        try
        {
            await this.viewModel.LoadDashboard(this.loadCancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void RefreshUi()
    {
        this.UserNameText.Text = this.viewModel.CurrentUser?.FullName ?? string.Empty;
        this.TransactionsList.ItemsSource = this.viewModel.RecentTransactionItems;
        this.EmptyTransactionsState.Visibility =
            this.viewModel.RecentTransactionItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        this.BuildCardDots();
        this.ShowCard(this.currentCardIndex);
        NavView.Current?.UpdateNotificationBadge(this.viewModel.UnreadNotificationCount);
    }

    private void ShowCard(int index)
    {
        var cards = this.viewModel.Cards;
        if (cards.Count == 0)
        {
            this.currentCardIndex = 0;
            this.CardVisual.Visibility = Visibility.Collapsed;
            this.EmptyCardsState.Visibility = Visibility.Visible;
            this.ClearCardDisplay();
            this.UpdateCardNavigationState();
            return;
        }

        index = Math.Clamp(index, 0, cards.Count - 1);
        this.currentCardIndex = index;
        this.CardVisual.Visibility = Visibility.Visible;
        this.EmptyCardsState.Visibility = Visibility.Collapsed;

        var card = cards[index];

        this.CardBankName.Text = "BankApp";
        this.CardBrandName.Text = string.IsNullOrWhiteSpace(card.CardBrand)
            ? card.CardType
            : card.CardBrand;
        this.CardHolderText.Text = string.IsNullOrWhiteSpace(card.CardholderName)
            ? "CARD HOLDER"
            : card.CardholderName.ToUpperInvariant();
        this.CardExpiryText.Text = card.ExpiryDate.ToString("MM/yy");
        this.CardNumberText.Text = this.MaskCardNumber(card.CardNumber);

        this.UpdateCardDots();
        this.UpdateCardNavigationState();
    }

    private void BuildCardDots()
    {
        this.CardDots.Children.Clear();
        var count = this.viewModel.Cards.Count;
        this.CardDots.Visibility = count > 1 ? Visibility.Visible : Visibility.Collapsed;
        for (var i = 0; i < count; i++)
        {
            var dot = new Ellipse
            {
                Width = i == this.currentCardIndex ? 18 : 8,
                Height = 8,
                Fill = new SolidColorBrush(
                    i == this.currentCardIndex
                    ? Color.FromArgb(255, 78, 205, 196)
                    : Color.FromArgb(100, 78, 205, 196)),
            };
            this.CardDots.Children.Add(dot);
        }
    }

    private void UpdateCardDots()
    {
        for (var i = 0; i < this.CardDots.Children.Count; i++)
        {
            if (this.CardDots.Children[i] is not Ellipse dot)
            {
                continue;
            }

            dot.Width = i == this.currentCardIndex ? 18 : 8;
            dot.Fill = new SolidColorBrush(
                i == this.currentCardIndex
                ? Color.FromArgb(255, 78, 205, 196)
                : Color.FromArgb(100, 78, 205, 196));
        }
    }

    private void PrevCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.currentCardIndex > 0)
        {
            this.ShowCard(this.currentCardIndex - 1);
        }
    }

    private void NextCardButton_Click(object sender, RoutedEventArgs e)
    {
        var count = this.viewModel.Cards.Count;
        if (this.currentCardIndex < count - 1)
        {
            this.ShowCard(this.currentCardIndex + 1);
        }
    }

    private void TransferButton_Click(object sender, RoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(() => this.ShowComingSoonAsync("Transfers"));
    }

    private void PayBillButton_Click(object sender, RoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(() => this.ShowComingSoonAsync("Bill Payments"));
    }

    private void ExchangeButton_Click(object sender, RoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(() => this.ShowComingSoonAsync("Currency Exchange"));
    }

    private void TxHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(() => this.ShowComingSoonAsync("Transaction History"));
    }

    private void ShowError(string msg)
    {
        this.ErrorInfoBar.Message = string.IsNullOrWhiteSpace(msg)
            ? "We couldn't load your dashboard right now."
            : msg;
        this.ErrorInfoBar.IsOpen = true;
    }

    private async Task ShowAlertAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot,
        };
        await dialog.ShowAsync();
    }

    private void CardVisual_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(this.ShowCurrentCardDetailsAsync);
    }

    private async Task ShowCurrentCardDetailsAsync()
    {
        var cards = this.viewModel.Cards;
        if (cards.Count == 0)
        {
            return;
        }

        var card = cards[this.currentCardIndex];

        var details =
            $"Card Type:     {card.CardType}\n" +
            $"Card Brand:    {card.CardBrand ?? "Mastercard"}\n" +
            $"Card Number:   {this.MaskCardNumber(card.CardNumber)}\n" +
            $"Cardholder:    {card.CardholderName}\n" +
            $"Expiry Date:   {card.ExpiryDate:MM/yy}\n" +
            $"Status:        {card.Status}\n" +
            $"Contactless:   {(card.IsContactlessEnabled ? "Enabled" : "Disabled")}\n" +
            $"Online Payments: {(card.IsOnlineEnabled ? "Enabled" : "Disabled")}";

        await this.ShowAlertAsync("Card Details", details);
    }

    private void ShowLoading()
    {
        this.LoadingOverlay.Visibility = Visibility.Visible;
    }

    private void HideLoading()
    {
        this.LoadingOverlay.Visibility = Visibility.Collapsed;
    }

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(this.LoadDashboardAsync);
    }

    private void AttachObserver()
    {
        if (this.isObserverAttached)
        {
            return;
        }

        this.viewModel.State.AddObserver(this);
        this.isObserverAttached = true;
    }

    private void DetachObserver()
    {
        if (!this.isObserverAttached)
        {
            return;
        }

        this.viewModel.State.RemoveObserver(this);
        this.isObserverAttached = false;
    }

    private void CancelPendingLoad()
    {
        if (this.loadCancellationTokenSource == null)
        {
            return;
        }

        this.loadCancellationTokenSource.Cancel();
        this.loadCancellationTokenSource.Dispose();
        this.loadCancellationTokenSource = null;
    }

    private void ClearCardDisplay()
    {
        this.CardBankName.Text = string.Empty;
        this.CardBrandName.Text = string.Empty;
        this.CardHolderText.Text = string.Empty;
        this.CardExpiryText.Text = string.Empty;
        this.CardNumberText.Text = "**** **** **** ****";
    }

    private void UpdateCardNavigationState()
    {
        var count = this.viewModel.Cards.Count;
        var hasCards = count > 0;

        this.PrevCardButton.IsEnabled = hasCards && this.currentCardIndex > 0;
        this.NextCardButton.IsEnabled = hasCards && this.currentCardIndex < count - 1;
        this.PrevCardButton.Visibility = hasCards ? Visibility.Visible : Visibility.Collapsed;
        this.NextCardButton.Visibility = hasCards ? Visibility.Visible : Visibility.Collapsed;
    }

    private string MaskCardNumber(string? cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return "**** **** **** ****";
        }

        return cardNumber.Length >= 4
            ? $"**** **** **** {cardNumber[^4..]}"
            : "**** **** **** ****";
    }

    private async Task ShowComingSoonAsync(string feature)
    {
        if (NavView.Current != null)
        {
            await NavView.Current.ShowComingSoonAsync(feature);
            return;
        }

        await this.ShowAlertAsync(feature, $"{feature} is coming soon.");
    }

    private async Task RunUiTaskAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            this.HideLoading();
            this.ShowError($"Unexpected error: {ex.Message}");
        }
    }
}
