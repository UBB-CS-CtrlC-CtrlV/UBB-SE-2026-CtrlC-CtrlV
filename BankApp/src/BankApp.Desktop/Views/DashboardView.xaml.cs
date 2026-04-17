// <copyright file="DashboardView.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace BankApp.Desktop.Views;

/// <summary>
/// Displays the authenticated user's account summary, card carousel, and recent transactions.
/// </summary>
public sealed partial class DashboardView : IStateObserver<DashboardState>
{
    private const int ActiveCardDotSize = 18;
    private const int InactiveCardDotSize = 8;
    private const int MinimumVisibleDotCount = 1;
    private const int FirstCardDotIndex = 0;
    private const byte ActiveDotAlpha = 255;
    private const byte InactiveDotAlpha = 100;
    private const byte DotRedChannel = 78;
    private const byte DotGreenChannel = 205;
    private const byte DotBlueChannel = 196;
    private static readonly Color ActiveDotColor = Color.FromArgb(ActiveDotAlpha, DotRedChannel, DotGreenChannel, DotBlueChannel);
    private static readonly Color InactiveDotColor = Color.FromArgb(InactiveDotAlpha, DotRedChannel, DotGreenChannel, DotBlueChannel);

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
        this.Loaded += this.OnPageLoaded;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        this.AttachObserver();
        _ = this.RunUiTaskAsync(this.LoadDashboardAsync);
    }

    /// <inheritdoc/>
    public void Update(DashboardState state)
    {
        this.OnStateChanged(state);
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

        // Visibility decided by ViewModel state; View only does the mapping to Visibility enum.
        this.EmptyTransactionsState.Visibility =
            this.viewModel.HasTransactions ? Visibility.Collapsed : Visibility.Visible;

        this.BuildCardDots();
        this.ShowCard();
        NavView.Current?.UpdateNotificationBadge(this.viewModel.UnreadNotificationCount);
    }

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
        var dots = this.viewModel.CardDots;
        this.CardDots.Visibility = dots.Count > MinimumVisibleDotCount ? Visibility.Visible : Visibility.Collapsed;
        foreach (var dotViewModel in dots)
        {
            var dot = new Ellipse
            {
                Width = dotViewModel.IsActive ? ActiveCardDotSize : InactiveCardDotSize,
                Height = InactiveCardDotSize,
                Fill = new SolidColorBrush(dotViewModel.IsActive ? ActiveDotColor : InactiveDotColor),
            };
            this.CardDots.Children.Add(dot);
        }
    }

    private void UpdateCardDots()
    {
        IReadOnlyList<CardPageIndicatorViewModel> dots = this.viewModel.CardDots;
        for (int index = FirstCardDotIndex; index < this.CardDots.Children.Count; index++)
        {
            if (this.CardDots.Children[index] is not Ellipse dot || index >= dots.Count)
            {
                continue;
            }

            dot.Width = dots[index].IsActive ? ActiveCardDotSize : InactiveCardDotSize;
            dot.Fill = new SolidColorBrush(dots[index].IsActive ? ActiveDotColor : InactiveDotColor);
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
        if (!this.viewModel.NavigatePrevious().IsError)
        {
            this.ShowCard();
        }
    }

    private void NextCardButton_Click(object sender, RoutedEventArgs e)
    {
        if (!this.viewModel.NavigateNext().IsError)
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

    private void TransactionHistoryButton_Click(object sender, RoutedEventArgs e)
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

    /// <summary>
    /// Shows a ContentDialog with the details of the currently selected card.
    /// The detail string is produced by the ViewModel; this method only handles the dialog UI.
    /// </summary>
    private async Task ShowCurrentCardDetailsAsync()
    {
        string details = this.viewModel.GetSelectedCardDetails();
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

    private void ShowError(string message)
    {
        this.ErrorInfoBar.Message = string.IsNullOrWhiteSpace(message)
            ? "We couldn't load your dashboard right now."
            : message;
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
        if (this.loadCancellationTokenSource is null)
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
        catch (Exception exception)
        {
            this.HideLoading();
            this.ShowError($"Unexpected error: {exception.Message}");
        }
    }
}
