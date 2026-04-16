// <copyright file="DashboardServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
using BankApp.Contracts.DTOs.Dashboard;
using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Dashboard;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace BankApp.Server.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="DashboardService"/>.
/// </summary>
public class DashboardServiceTests
{
    private readonly IDashboardRepository dashboardRepository = Substitute.For<IDashboardRepository>();
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly DashboardService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardServiceTests"/> class.
    /// </summary>
    public DashboardServiceTests()
    {
        this.service = new DashboardService(
            this.dashboardRepository,
            this.userRepository,
            NullLogger<DashboardService>.Instance);
    }

    [Fact]
    public void GetDashboardData_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void GetDashboardData_WhenUserExists_ReturnsResponseWithUserSummary()
    {
        // Arrange
        const int userId = 1;
        const string fullName = "Ada Lovelace";
        const string email = "ada@lovelace.com";
        var user = new User { Id = userId, FullName = fullName, Email = email };
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(new List<Card>());
        this.dashboardRepository.GetAccountsByUser(userId).Returns(new List<Account>());
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(0);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.CurrentUser!.FullName.Should().Be(fullName);
        result.Value.CurrentUser!.Email.Should().Be(email);
    }

    [Fact]
    public void GetDashboardData_WhenCardsExist_ReturnsMappedCards()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, FullName = "Ada", Email = "ada@test.com" };
        var cards = new List<Card>
        {
            new Card
            {
                Id = 1,
                UserId = userId,
                CardNumber = "1234567890123456",
                CardholderName = "Ada Lovelace",
                CardType = CardType.Debit,
                ExpiryDate = new DateTime(2027, 12, 1),
                Status = CardStatus.Active,
            },
        };
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(cards);
        this.dashboardRepository.GetAccountsByUser(userId).Returns(new List<Account>());
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(0);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Cards.Should().ContainSingle();
        result.Value.Cards[0].CardholderName.Should().Be("Ada Lovelace");
        result.Value.Cards[0].CardType.Should().Be(CardType.Debit);
    }

    [Fact]
    public void GetDashboardData_WhenCardsQueryFails_ReturnsEmptyCardList()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, FullName = "Ada", Email = "ada@test.com" };
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(Error.Failure());
        this.dashboardRepository.GetAccountsByUser(userId).Returns(new List<Account>());
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(0);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Cards.Should().BeEmpty();
    }

    [Fact]
    public void GetDashboardData_WhenTransactionsExist_ReturnsMappedTransactions()
    {
        // Arrange
        const int userId = 1;
        const int accountId = 10;
        var user = new User { Id = userId, FullName = "Ada", Email = "ada@test.com" };
        var accounts = new List<Account>
        {
            new Account { Id = accountId, UserId = userId },
        };
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = 1,
                AccountId = accountId,
                Direction = TransactionDirection.Out,
                Amount = 100,
                Currency = "RON",
                Status = TransactionStatus.Completed,
                MerchantName = "Shop",
                CreatedAt = DateTime.UtcNow,
            },
        };
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(new List<Card>());
        this.dashboardRepository.GetAccountsByUser(userId).Returns(accounts);
        this.dashboardRepository.GetRecentTransactions(accountId, Arg.Any<int>()).Returns(transactions);
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(0);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RecentTransactions.Should().ContainSingle();
        result.Value.RecentTransactions[0].MerchantName.Should().Be("Shop");
        result.Value.RecentTransactions[0].Amount.Should().Be(100);
    }

    [Fact]
    public void GetDashboardData_WhenAccountsQueryFails_ReturnsEmptyTransactionList()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, FullName = "Ada", Email = "ada@test.com" };
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(new List<Card>());
        this.dashboardRepository.GetAccountsByUser(userId).Returns(Error.Failure());
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(0);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RecentTransactions.Should().BeEmpty();
    }

    [Fact]
    public void GetDashboardData_WhenNotificationCountQueryFails_ReturnsZeroCount()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, FullName = "Ada", Email = "ada@test.com" };
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(new List<Card>());
        this.dashboardRepository.GetAccountsByUser(userId).Returns(new List<Account>());
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(Error.Failure());

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UnreadNotificationCount.Should().Be(0);
    }

    [Fact]
    public void GetDashboardData_WhenMultipleAccountsExist_MergesAndLimitsTransactions()
    {
        // Arrange
        const int userId = 1;
        const int accountId1 = 10;
        const int accountId2 = 11;
        var user = new User { Id = userId, FullName = "Ada", Email = "ada@test.com" };
        var accounts = new List<Account>
        {
            new Account { Id = accountId1, UserId = userId },
            new Account { Id = accountId2, UserId = userId },
        };
        var transactions1 = Enumerable.Range(1, 8).Select(i => new Transaction
        {
            Id = i,
            AccountId = accountId1,
            Direction = TransactionDirection.In,
            Amount = i * 10,
            Currency = "RON",
            Status = TransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddMinutes(-i),
        }).ToList();
        var transactions2 = Enumerable.Range(9, 5).Select(i => new Transaction
        {
            Id = i,
            AccountId = accountId2,
            Direction = TransactionDirection.Out,
            Amount = i * 10,
            Currency = "RON",
            Status = TransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddMinutes(-i),
        }).ToList();
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(new List<Card>());
        this.dashboardRepository.GetAccountsByUser(userId).Returns(accounts);
        this.dashboardRepository.GetRecentTransactions(accountId1, Arg.Any<int>()).Returns(transactions1);
        this.dashboardRepository.GetRecentTransactions(accountId2, Arg.Any<int>()).Returns(transactions2);
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(0);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.RecentTransactions.Should().HaveCount(10);
    }

    [Fact]
    public void GetDashboardData_WhenUnreadNotificationsExist_ReturnsCorrectCount()
    {
        // Arrange
        const int userId = 1;
        const int unreadCount = 7;
        var user = new User { Id = userId, FullName = "Ada", Email = "ada@test.com" };
        this.userRepository.FindById(userId).Returns(user);
        this.dashboardRepository.GetCardsByUser(userId).Returns(new List<Card>());
        this.dashboardRepository.GetAccountsByUser(userId).Returns(new List<Account>());
        this.dashboardRepository.GetUnreadNotificationCount(userId).Returns(unreadCount);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UnreadNotificationCount.Should().Be(unreadCount);
    }
}