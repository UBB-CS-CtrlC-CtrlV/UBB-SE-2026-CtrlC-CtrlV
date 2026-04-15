using System.Globalization;
using BankApp.Client.Enums;
using BankApp.Client.Utilities;
using BankApp.Client.ViewModels;
using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.Enums;
using ErrorOr;
using FluentAssertions;
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
        const string email = "ada@lovelace.com";
        const string cardBrand = "Visa";
        const CardType cardType = CardType.Debit;
        const string cardNumber = "1234567812345678";
        var cardExpiry = new DateTime(2027, 12, 1);
        const string merchantName = "Coffee Shop";
        const string currency = "USD";
        const decimal transactionAmount = 12.5m;
        const int unreadCount = 4;

        var response = new DashboardResponse
        {
            CurrentUser = new UserSummaryDto
            {
                FullName = fullName,
                Email = email,
            },
            Cards =
            [
                new CardDto
                {
                    CardBrand = cardBrand,
                    CardType = cardType,
                    CardholderName = fullName,
                    CardNumber = cardNumber,
                    ExpiryDate = cardExpiry,
                    Status = CardStatus.Active,
                    IsContactlessEnabled = true,
                    IsOnlineEnabled = true,
                },
            ],
            RecentTransactions =
            [
                new TransactionDto
                {
                    MerchantName = merchantName,
                    Direction = TransactionDirection.Out,
                    Amount = transactionAmount,
                    Currency = currency,
                },
            ],
            UnreadNotificationCount = unreadCount,
        };
        this.apiClient.GetAsync<DashboardResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<ErrorOr<DashboardResponse>>(response));

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert — result and state
        result.IsError.Should().BeFalse();
        this.viewModel.State.Value.Should().Be(DashboardState.Success);
        this.viewModel.ErrorMessage.Should().BeEmpty();

        // Assert — current user
        this.viewModel.CurrentUser.Should().BeEquivalentTo(new UserSummaryDto
        {
            FullName = fullName,
            Email = email,
        });

        // Assert — selected card display properties
        this.viewModel.CardDots.Should().ContainSingle();
        this.viewModel.SelectedCardBrandDisplay.Should().Be(cardBrand);
        this.viewModel.SelectedCardHolderDisplay.Should().Be(fullName.ToUpperInvariant());
        this.viewModel.SelectedCardNumberMasked.Should().Be($"**** **** **** {cardNumber[^4..]}");
        this.viewModel.SelectedCardExpiryDisplay.Should().Be(cardExpiry.ToString("MM/yy"));

        // Assert — transaction item
        string expectedAmountDisplay = $"-{transactionAmount.ToString("N2", CultureInfo.InvariantCulture)}";
        this.viewModel.RecentTransactionItems.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new DashboardTransactionItem
            {
                MerchantDisplayName = merchantName,
                AmountDisplay = expectedAmountDisplay,
                Currency = currency,
            });

        // Assert — notification count
        this.viewModel.UnreadNotificationCount.Should().Be(unreadCount);
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
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.IncompleteResponse);
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
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.SessionExpired);
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
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.NotFound);
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
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.LoadFailed);
    }

    [Fact]
    public async Task NavigatePrevious_WhenNoCardsAreLoaded_ReturnsError()
    {
        // Arrange
        await this.LoadViewModelWithCards(0);

        // Act
        ErrorOr<Success> result = this.viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task NavigatePrevious_WhenAtFirstCard_ReturnsError()
    {
        // Arrange
        await this.LoadViewModelWithCards(2);

        // Act
        ErrorOr<Success> result = this.viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeTrue();
        this.viewModel.CurrentCardIndex.Should().Be(0);
    }

    [Fact]
    public async Task NavigatePrevious_WhenNotAtFirstCard_SucceedsAndDecrementsIndex()
    {
        // Arrange
        await this.LoadViewModelWithCards(2);
        this.viewModel.NavigateNext();

        // Act
        ErrorOr<Success> result = this.viewModel.NavigatePrevious();

        // Assert
        result.IsError.Should().BeFalse();
        this.viewModel.CurrentCardIndex.Should().Be(0);
    }

    [Fact]
    public async Task NavigateNext_WhenNoCardsAreLoaded_ReturnsError()
    {
        // Arrange
        await this.LoadViewModelWithCards(0);

        // Act
        ErrorOr<Success> result = this.viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task NavigateNext_WhenAtLastCard_ReturnsError()
    {
        // Arrange
        await this.LoadViewModelWithCards(1);

        // Act
        ErrorOr<Success> result = this.viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeTrue();
        this.viewModel.CurrentCardIndex.Should().Be(0);
    }

    [Fact]
    public async Task NavigateNext_WhenNotAtLastCard_SucceedsAndIncrementsIndex()
    {
        // Arrange
        await this.LoadViewModelWithCards(2);

        // Act
        ErrorOr<Success> result = this.viewModel.NavigateNext();

        // Assert
        result.IsError.Should().BeFalse();
        this.viewModel.CurrentCardIndex.Should().Be(1);
    }

    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsPresent_ReturnsBrand()
    {
        // Arrange
        const string cardBrand = "Visa";
        await this.LoadViewModelWithCards(1, cardBrand: cardBrand);

        // Act
        string display = this.viewModel.SelectedCardBrandDisplay;

        // Assert
        display.Should().Be(cardBrand);
    }

    [Fact]
    public async Task SelectedCardBrandDisplay_WhenBrandIsAbsent_FallsBackToCardType()
    {
        // Arrange
        const CardType cardType = CardType.Credit;
        await this.LoadViewModelWithCards(1, cardBrand: string.Empty, cardType: cardType);

        // Act
        string display = this.viewModel.SelectedCardBrandDisplay;

        // Assert
        display.Should().Be(cardType.ToString());
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
        display.Should().Be(cardholderName.ToUpperInvariant());
    }

    [Fact]
    public async Task SelectedCardHolderDisplay_WhenNameIsAbsent_ReturnsPlaceholder()
    {
        // Arrange
        const string expectedSelectedCardHolderDisplay = "CARD HOLDER";
        await this.LoadViewModelWithCards(1, cardholderName: string.Empty);

        // Act
        string display = this.viewModel.SelectedCardHolderDisplay;

        // Assert
        display.Should().Be(expectedSelectedCardHolderDisplay);
    }

    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsValid_ShowsOnlyLastFourDigits()
    {
        // Arrange
        const string cardNumber = "1234567890123456";
        string expectedMaskedCardNumber = $"**** **** **** {cardNumber[^4..]}";
        await this.LoadViewModelWithCards(1, cardNumber: cardNumber);

        // Act
        string masked = this.viewModel.SelectedCardNumberMasked;

        // Assert
        masked.Should().Be(expectedMaskedCardNumber);
    }

    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsTooShort_ReturnsFullyMasked()
    {
        // Arrange
        const string expectedMaskedCardNumber = "**** **** **** ****";
        await this.LoadViewModelWithCards(1, cardNumber: "123");

        // Act
        string masked = this.viewModel.SelectedCardNumberMasked;

        // Assert
        masked.Should().Be(expectedMaskedCardNumber);
    }

    [Fact]
    public void GetSelectedCardDetails_WhenNoCardIsSelected_ReturnsEmptyString()
    {
        // Arrange - viewModel starts with no cards loaded

        // Act
        string details = this.viewModel.GetSelectedCardDetails();

        // Assert
        details.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSelectedCardDetails_WhenCardIsSelected_ReturnsFormattedDetails()
    {
        // Arrange
        const CardType cardType = CardType.Debit;
        const string cardBrand = "Visa";
        const string cardNumber = "1234567890123456";
        const string cardholderName = "Ada Lovelace";
        string expectedMaskedCardNumber = $"**** **** **** {cardNumber[^4..]}";
        await this.LoadViewModelWithCards(
            cardCount: 1,
            cardType: cardType,
            cardBrand: cardBrand,
            cardNumber: cardNumber,
            cardholderName: cardholderName);

        // Act
        string details = this.viewModel.GetSelectedCardDetails();

        // Assert
        details.Should().Contain(cardType.ToString())
               .And.Contain(cardBrand)
               .And.Contain(expectedMaskedCardNumber)
               .And.Contain(cardholderName);
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
        dots.Select(d => d.IsActive).Should().Equal(false, true, false);
    }

    private async Task LoadViewModelWithCards(
        int cardCount,
        string cardBrand = "Visa",
        CardType cardType = CardType.Debit,
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
