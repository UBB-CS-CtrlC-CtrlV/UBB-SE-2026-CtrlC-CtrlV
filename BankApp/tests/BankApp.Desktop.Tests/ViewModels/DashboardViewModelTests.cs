// <copyright file="DashboardViewModelTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Globalization;
using BankApp.Application.DTOs.Dashboard;
using BankApp.Desktop.Enums;
using BankApp.Desktop.Utilities;
using BankApp.Desktop.ViewModels;
using BankApp.Domain.Enums;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace BankApp.Desktop.Tests.ViewModels;

/// <summary>
/// Tests for the <see cref="DashboardViewModel"/>.
/// </summary>
public class DashboardViewModelTests
{
    private readonly Mock<IApiClient> apiClient;
    private readonly DashboardViewModel viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModelTests"/> class.
    /// Creates a fresh mock and view model for each test.
    /// </summary>
    public DashboardViewModelTests()
    {
        this.apiClient = new Mock<IApiClient>(MockBehavior.Strict);
        this.viewModel = new DashboardViewModel(this.apiClient.Object, NullLogger<DashboardViewModel>.Instance);
    }

    /// <summary>
    /// In LoadDashboard, when the response is valid the view model should be populated.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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
        this.apiClient
            .Setup(client => client.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert — result and state
        result.IsError.Should().BeFalse();
        this.viewModel.State.Value.Should().Be(DashboardState.Success);
        this.viewModel.ErrorMessage.Should().BeEmpty();

        // Assert — current user
        this.viewModel.CurrentUser.Should().BeEquivalentTo(
            new UserSummaryDto
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
        var expectedAmountDisplay = $"-{transactionAmount.ToString("N2", CultureInfo.InvariantCulture)}";
        this.viewModel.RecentTransactionItems.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(
                new DashboardTransactionItem
            {
                MerchantDisplayName = merchantName,
                AmountDisplay = expectedAmountDisplay,
                Currency = currency,
            });

        // Assert — notification count
        this.viewModel.UnreadNotificationCount.Should().Be(unreadCount);
    }

    /// <summary>
    /// In LoadDashboard(), when the current user is missing the error state should be set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenCurrentUserIsMissing_SetsErrorState()
    {
        // Arrange
        this.apiClient
            .Setup(client => client.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DashboardResponse());

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.IncompleteResponse);
    }

    /// <summary>
    /// In LoadDashboard, when the request is unauthorized the session expired message should be set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenUnauthorized_SetsSessionExpiredMessage()
    {
        // Arrange
        this.apiClient
            .Setup(client => client.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Unauthorized());

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.SessionExpired);
    }

    /// <summary>
    /// In LoadDashboard, when the request returns not found the not found message should be set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenNotFound_SetsNotFoundMessage()
    {
        // Arrange
        this.apiClient
            .Setup(client => client.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.NotFound());

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.NotFound);
    }

    /// <summary>
    /// In LoadDashboard, when the API returns a general failure the load failed message should be set.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task LoadDashboard_WhenApiFailureOccurs_SetsLoadFailedMessage()
    {
        // Arrange
        this.apiClient
            .Setup(client => client.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure());

        // Act
        ErrorOr<Success> result = await this.viewModel.LoadDashboard();

        // Assert
        result.IsError.Should().BeTrue();
        this.viewModel.State.Value.Should().Be(DashboardState.Error);
        this.viewModel.ErrorMessage.Should().Be(UserMessages.Dashboard.LoadFailed);
    }

    /// <summary>
    /// In NavigatePrevious, when no cards are loaded an error should be returned.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// In NavigatePrevious, when already at the first card an error should be returned
    /// and the card index should remain at zero.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// In NavigatePrevious, when not at the first card the operation should succeed
    /// and the card index should decrement by one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// In NavigateNext, when no cards are loaded an error should be returned.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// In NavigateNext, when already at the last card an error should be returned
    /// and the card index should remain unchanged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// In NavigateNext, when not at the last card the operation should succeed
    /// and the card index should increment by one.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// When a card brand is set, <see cref="DashboardViewModel.SelectedCardBrandDisplay"/>
    /// should return the brand name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// When the card brand is empty, <see cref="DashboardViewModel.SelectedCardBrandDisplay"/>
    /// should fall back to the card type string.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// When a cardholder name is set, <see cref="DashboardViewModel.SelectedCardHolderDisplay"/>
    /// should return the name in upper case.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// When the cardholder name is empty, <see cref="DashboardViewModel.SelectedCardHolderDisplay"/>
    /// should return a placeholder string.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// When a valid card number is set, <see cref="DashboardViewModel.SelectedCardNumberMasked"/>
    /// should expose only the last four digits and mask the rest.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task SelectedCardNumberMasked_WhenCardNumberIsValid_ShowsOnlyLastFourDigits()
    {
        // Arrange
        const string cardNumber = "1234567890123456";
        var expectedMaskedCardNumber = $"**** **** **** {cardNumber[^4..]}";
        await this.LoadViewModelWithCards(1, cardNumber: cardNumber);

        // Act
        string masked = this.viewModel.SelectedCardNumberMasked;

        // Assert
        masked.Should().Be(expectedMaskedCardNumber);
    }

    /// <summary>
    /// When the card number is too short to extract four digits,
    /// <see cref="DashboardViewModel.SelectedCardNumberMasked"/> should return a fully masked string.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
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

    /// <summary>
    /// When no cards are loaded, <see cref="DashboardViewModel.GetSelectedCardDetails"/>
    /// should return an empty string.
    /// </summary>
    [Fact]
    public void GetSelectedCardDetails_WhenNoCardIsSelected_ReturnsEmptyString()
    {
        // Arrange — viewModel starts with no cards loaded

        // Act
        string details = this.viewModel.GetSelectedCardDetails();

        // Assert
        details.Should().BeEmpty();
    }

    /// <summary>
    /// When a card is selected, <see cref="DashboardViewModel.GetSelectedCardDetails"/> should return
    /// a string containing the card type, brand, masked number, and cardholder name.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetSelectedCardDetails_WhenCardIsSelected_ReturnsFormattedDetails()
    {
        // Arrange
        const CardType cardType = CardType.Debit;
        const string cardBrand = "Visa";
        const string cardNumber = "1234567890123456";
        const string cardholderName = "Ada Lovelace";
        var expectedMaskedCardNumber = $"**** **** **** {cardNumber[^4..]}";
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

    /// <summary>
    /// When the second card is selected via navigation, only the second card dot should be active.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task CardDots_WhenNavigatedToSecondCard_SecondDotIsActive()
    {
        // Arrange
        await this.LoadViewModelWithCards(3);
        this.viewModel.NavigateNext();

        // Act
        IReadOnlyList<CardPageIndicatorViewModel> dots = this.viewModel.CardDots;

        // Assert
        dots.Select(dot => dot.IsActive).Should().Equal(false, true, false);
    }

    private async Task LoadViewModelWithCards(
        int cardCount,
        string cardBrand = "Visa",
        CardType cardType = CardType.Debit,
        string cardholderName = "Test User",
        string cardNumber = "1234567812345678")
    {
        List<CardDto> cards = Enumerable.Range(0, cardCount)
            .Select(index => new CardDto
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

        this.apiClient
            .Setup(client => client.GetAsync<DashboardResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        await this.viewModel.LoadDashboard();
    }
}
