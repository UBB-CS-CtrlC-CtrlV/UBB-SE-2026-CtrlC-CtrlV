using System.Globalization;
using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Contracts.DTOs.Dashboard;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace BankApp.Client.Tests;

/// <summary>
/// Tests for the <see cref="DashboardViewModel"/>.
/// </summary>
public class DashboardViewModelTests
{
    private readonly ApiClient apiClient;
    private readonly DashboardViewModel viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModelTests"/> class.
    /// Creates a fresh substitute and view model for each test.
    /// </summary>
    public DashboardViewModelTests()
    {
        this.apiClient = Substitute.For<ApiClient>(new ConfigurationBuilder().Build(), NullLogger<ApiClient>.Instance);
        this.viewModel = new DashboardViewModel(this.apiClient, NullLogger<DashboardViewModel>.Instance);
    }

    [Fact]
    public async Task LoadDashboard_WhenResponseIsValid_PopulatesViewModel()
    {
        // Arrange
        const string fullName = "Ada Lovelace";
        const string merchantName = "Coffee Shop";
        const decimal transactionAmount = 12.5m;
        const int unreadCount = 4;
        var expectedAmountDisplay = $"-{transactionAmount.ToString("N2", CultureInfo.InvariantCulture)}";
        var response = new DashboardResponse
        {
            CurrentUser = new UserSummaryDto { FullName = fullName },
            Cards =
            [
                new CardDto
                {
                    CardBrand = "Visa", CardType = "Debit",
                    CardholderName = fullName, CardNumber = "1234567812345678",
                },
            ],
            RecentTransactions =
            [
                new TransactionDto
                {
                    MerchantName = merchantName, Type = "Card payment",
                    Direction = "Out", Amount = transactionAmount, Currency = "USD",
                },
            ],
            UnreadNotificationCount = unreadCount,
        };
        this.apiClient.GetAsync<DashboardResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<DashboardResponse>>(response));

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(DashboardState.Success, this.viewModel.State.Value);
        Assert.Equal(fullName, this.viewModel.CurrentUser?.FullName);
        Assert.Single(this.viewModel.Cards);
        Assert.Single(this.viewModel.RecentTransactionItems);
        Assert.Equal(expectedAmountDisplay, this.viewModel.RecentTransactionItems[0].AmountDisplay);
        Assert.Equal(unreadCount, this.viewModel.UnreadNotificationCount);
        Assert.Equal(string.Empty, this.viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadDashboard_WhenCurrentUserIsMissing_SetsErrorState()
    {
        // Arrange
        this.apiClient.GetAsync<DashboardResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<DashboardResponse>>(new DashboardResponse()));

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(DashboardState.Error, this.viewModel.State.Value);
        Assert.Equal(UserMessages.Dashboard.IncompleteResponse, this.viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadDashboard_WhenUnauthorized_SetsSessionExpiredMessage()
    {
        // Arrange
        this.apiClient.GetAsync<DashboardResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<DashboardResponse>>(Error.Unauthorized()));

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(DashboardState.Error, this.viewModel.State.Value);
        Assert.Equal(UserMessages.Dashboard.SessionExpired, this.viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadDashboard_WhenNotFound_SetsNotFoundMessage()
    {
        // Arrange
        this.apiClient.GetAsync<DashboardResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<DashboardResponse>>(Error.NotFound()));

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(DashboardState.Error, this.viewModel.State.Value);
        Assert.Equal(UserMessages.Dashboard.NotFound, this.viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadDashboard_WhenApiFailureOccurs_SetsLoadFailedMessage()
    {
        // Arrange
        this.apiClient.GetAsync<DashboardResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<DashboardResponse>>(Error.Failure()));

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(DashboardState.Error, this.viewModel.State.Value);
        Assert.Equal(UserMessages.Dashboard.LoadFailed, this.viewModel.ErrorMessage);
    }

    [Fact]
    public async Task NavigatePrevious_WhenNoCardsAreLoaded_ReturnsFalse()
    {
        // Arrange
        await this.LoadViewModelWithCards(0);

        // Act
        bool result = this.viewModel.NavigatePrevious();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task NavigatePrevious_WhenAtFirstCard_ReturnsFalse()
    {
        // Arrange
        await this.LoadViewModelWithCards(2);

        // Act
        bool result = this.viewModel.NavigatePrevious();

        // Assert
        Assert.False(result);
        Assert.Equal(0, this.viewModel.CurrentCardIndex);
    }

    [Fact]
    public async Task NavigatePrevious_WhenNotAtFirstCard_ReturnsTrueAndDecrementsIndex()
    {
        // Arrange
        await this.LoadViewModelWithCards(2);
        this.viewModel.NavigateNext();

        // Act
        bool result = this.viewModel.NavigatePrevious();

        // Assert
        Assert.True(result);
        Assert.Equal(0, this.viewModel.CurrentCardIndex);
    }

    [Fact]
    public async Task NavigateNext_WhenNoCardsAreLoaded_ReturnsFalse()
    {
        // Arrange
        await this.LoadViewModelWithCards(0);

        // Act
        bool result = this.viewModel.NavigateNext();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task NavigateNext_WhenAtLastCard_ReturnsFalse()
    {
        // Arrange
        await this.LoadViewModelWithCards(1);

        // Act
        bool result = this.viewModel.NavigateNext();

        // Assert
        Assert.False(result);
        Assert.Equal(0, this.viewModel.CurrentCardIndex);
    }

    [Fact]
    public async Task NavigateNext_WhenNotAtLastCard_ReturnsTrueAndIncrementsIndex()
    {
        // Arrange
        await this.LoadViewModelWithCards(2);

        // Act
        bool result = this.viewModel.NavigateNext();

        // Assert
        Assert.True(result);
        Assert.Equal(1, this.viewModel.CurrentCardIndex);
    }

    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsPresent_ReturnsBrand()
    {
        // Arrange
        const string cardBrand = "Visa";
        await this.LoadViewModelWithCards(1, cardBrand: cardBrand, cardType: "Debit");

        // Act
        string display = this.viewModel.SelectedCardBrandDisplay;

        // Assert
        Assert.Equal(cardBrand, display);
    }

    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsAbsent_FallsBackToCardType()
    {
        // Arrange
        const string cardType = "Credit";
        await this.LoadViewModelWithCards(1, cardBrand: string.Empty, cardType: cardType);

        // Act
        string display = this.viewModel.SelectedCardBrandDisplay;

        // Assert
        Assert.Equal(cardType, display);
    }

    [Fact]
    public async Task SelectedCardHolderDisplay_WhenNameIsPresent_ReturnsUpperCasedName()
    {
        // Arrange
        const string cardholderName = "Ada Lovelace";
        await this.LoadViewModelWithCards(1, cardholderName: cardholderName);

        // Act
        string display = this.viewModel.SelectedCardHolderDisplay;

        // Assert
        Assert.Equal(cardholderName.ToUpperInvariant(), display);
    }

    [Fact]
    public async Task SelectedCardHolderDisplay_WhenNameIsAbsent_ReturnsPlaceholder()
    {
        // Arrange
        await this.LoadViewModelWithCards(1, cardholderName: string.Empty);

        // Act
        string display = this.viewModel.SelectedCardHolderDisplay;

        // Assert
        Assert.Equal("CARD HOLDER", display);
    }

    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsValid_ShowsOnlyLastFourDigits()
    {
        // Arrange
        const string cardNumber = "1234567890123456";
        await this.LoadViewModelWithCards(1, cardNumber: cardNumber);

        // Act
        string masked = this.viewModel.SelectedCardNumberMasked;

        // Assert
        Assert.Equal($"**** **** **** {cardNumber[^4..]}", masked);
    }

    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsTooShort_ReturnsFullyMasked()
    {
        // Arrange
        await this.LoadViewModelWithCards(1, cardNumber: "123");

        // Act
        string masked = this.viewModel.SelectedCardNumberMasked;

        // Assert
        Assert.Equal("**** **** **** ****", masked);
    }

    [Fact]
    public void GetSelectedCardDetails_WhenNoCardIsSelected_ReturnsEmptyString()
    {
        // Arrange - viewModel starts with no cards loaded

        // Act
        string details = this.viewModel.GetSelectedCardDetails();

        // Assert
        Assert.Equal(string.Empty, details);
    }

    [Fact]
    public async Task GetSelectedCardDetails_WhenCardIsSelected_ReturnsFormattedDetails()
    {
        // Arrange
        const string cardType = "Debit";
        const string cardBrand = "Visa";
        const string cardNumber = "1234567890123456";
        const string cardholderName = "Ada Lovelace";
        await this.LoadViewModelWithCards(
            cardCount: 1,
            cardType: cardType,
            cardBrand: cardBrand,
            cardNumber: cardNumber,
            cardholderName: cardholderName);

        // Act
        string details = this.viewModel.GetSelectedCardDetails();

        // Assert
        Assert.Contains(cardType, details);
        Assert.Contains(cardBrand, details);
        Assert.Contains($"**** **** **** {cardNumber[^4..]}", details);
        Assert.Contains(cardholderName, details);
    }

    [Fact]
    public async Task CardDots_WhenNavigatedToSecondCard_SecondDotIsActive()
    {
        // Arrange
        await this.LoadViewModelWithCards(3);
        this.viewModel.NavigateNext();

        // Act
        IReadOnlyList<CardPageIndicatorViewModel> dots = this.viewModel.CardDots;

        // Assert
        Assert.False(dots[0].IsActive);
        Assert.True(dots[1].IsActive);
        Assert.False(dots[2].IsActive);
    }

    private async Task LoadViewModelWithCards(
        int cardCount,
        string cardBrand = "Visa",
        string cardType = "Debit",
        string cardholderName = "Test User",
        string cardNumber = "1234567812345678")
    {
        List<CardDto> cards = Enumerable.Range(0, cardCount)
            .Select(_ => new CardDto
            {
                CardBrand = cardBrand,
                CardType = cardType,
                CardholderName = cardholderName,
                CardNumber = cardNumber,
            })
            .ToList();

        var response = new DashboardResponse
        {
            CurrentUser = new UserSummaryDto { FullName = "Test User" },
            Cards = cards,
        };

        this.apiClient.GetAsync<DashboardResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<DashboardResponse>>(response));

        await this.viewModel.LoadDashboard();
    }
}
