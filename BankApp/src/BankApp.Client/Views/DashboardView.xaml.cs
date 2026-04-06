// <copyright file="DashboardView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Client.Enums;
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

    // ─── State Handling ────────────────────────────────────────────────────────

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

        // Visibility decided by ViewModel state; View only does the mapping to Visibility enum.
        this.EmptyTransactionsState.Visibility =
            this.viewModel.HasTransactions ? Visibility.Collapsed : Visibility.Visible;

        this.BuildCardDots();
        this.ShowCard();
        NavView.Current?.UpdateNotificationBadge(this.viewModel.UnreadNotificationCount);
    }

    // ─── Card Display ──────────────────────────────────────────────────────────

    /// <summary>
    /// Renders the card at the current index stored in the ViewModel.
    /// All data and formatting decisions come from the ViewModel.
    /// </summary>
    private void ShowCard()
    {
        if (!this.viewModel.HasCards)
        {
            this.CardVisual.Visibility = Visibility.Collapsed;
            this.EmptyCardsState.Visibility = Visibility.Visible;
            this.ClearCardDisplay();
            this.UpdateCardNavigationState();
            return;
        }

        this.CardVisual.Visibility = Visibility.Visible;
        this.EmptyCardsState.Visibility = Visibility.Collapsed;

        // All formatting decisions are delegated to the ViewModel.
        this.CardBankName.Text = "BankApp";
        this.CardBrandName.Text = this.viewModel.SelectedCardBrandDisplay;
        this.CardHolderText.Text = this.viewModel.SelectedCardHolderDisplay;
        this.CardExpiryText.Text = this.viewModel.SelectedCardExpiryDisplay;
        this.CardNumberText.Text = this.viewModel.SelectedCardNumberMasked;

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
                Width = i == this.viewModel.CurrentCardIndex ? 18 : 8,
                Height = 8,
                Fill = new SolidColorBrush(
                    i == this.viewModel.CurrentCardIndex
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

            dot.Width = i == this.viewModel.CurrentCardIndex ? 18 : 8;
            dot.Fill = new SolidColorBrush(
                i == this.viewModel.CurrentCardIndex
                ? Color.FromArgb(255, 78, 205, 196)
                : Color.FromArgb(100, 78, 205, 196));
        }
    }

    private void UpdateCardNavigationState()
    {
        // ViewModel decides whether navigation is possible; View maps booleans to UI properties.
        this.PrevCardButton.IsEnabled = this.viewModel.CanNavigatePrevious;
        this.NextCardButton.IsEnabled = this.viewModel.CanNavigateNext;
        this.PrevCardButton.Visibility = this.viewModel.HasCards ? Visibility.Visible : Visibility.Collapsed;
        this.NextCardButton.Visibility = this.viewModel.HasCards ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ClearCardDisplay()
    {
        this.CardBankName.Text = string.Empty;
        this.CardBrandName.Text = string.Empty;
        this.CardHolderText.Text = string.Empty;
        this.CardExpiryText.Text = string.Empty;
        this.CardNumberText.Text = "**** **** **** ****";
    }

    private void PrevCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.viewModel.NavigatePrevious())
        {
            this.ShowCard();
        }
    }

    private void NextCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.viewModel.NavigateNext())
        {
            this.ShowCard();
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

    private void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(this.LoadDashboardAsync);
    }

    private void CardVisual_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _ = this.RunUiTaskAsync(this.ShowCurrentCardDetailsAsync);
    }

    // ─── Dialog Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Shows a ContentDialog with the details of the currently selected card.
    /// The detail string is produced by the ViewModel; this method only handles the dialog UI.
    /// </summary>
    private async Task ShowCurrentCardDetailsAsync()
    {
        var details = this.viewModel.GetSelectedCardDetails();
        if (string.IsNullOrEmpty(details))
        {
            return;
        }

        await this.ShowAlertAsync("Card Details", details);
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

    private async Task ShowComingSoonAsync(string feature)
    {
        if (NavView.Current != null)
        {
            await NavView.Current.ShowComingSoonAsync(feature);
            return;
        }

        await this.ShowAlertAsync(feature, $"{feature} is coming soon.");
    }

    private void ShowLoading()
    {
        this.LoadingOverlay.Visibility = Visibility.Visible;
    }

    private void HideLoading()
    {
        this.LoadingOverlay.Visibility = Visibility.Collapsed;
    }

    private void ShowError(string msg)
    {
        this.ErrorInfoBar.Message = string.IsNullOrWhiteSpace(msg)
            ? "We couldn't load your dashboard right now."
            : msg;
        this.ErrorInfoBar.IsOpen = true;
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
