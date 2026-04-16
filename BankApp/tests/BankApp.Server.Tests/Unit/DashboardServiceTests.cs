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
using Moq;

namespace BankApp.Server.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="DashboardService"/>.
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<IUserRepository> userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
    private readonly Mock<IDashboardRepository> dashboardRepository = new Mock<IDashboardRepository>(MockBehavior.Strict);
    private readonly DashboardService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardServiceTests"/> class.
    /// </summary>
    public DashboardServiceTests()
    {
        this.userRepository.Setup(repository => repository.FindById(It.IsAny<int>())).Returns(Error.NotFound());
        this.dashboardRepository.Setup(repository => repository.GetCardsByUser(It.IsAny<int>())).Returns(new List<Card>());
        this.dashboardRepository.Setup(repository => repository.GetAccountsByUser(It.IsAny<int>())).Returns(new List<Account>());
        this.dashboardRepository.Setup(repository => repository.GetRecentTransactions(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<Transaction>());
        this.dashboardRepository.Setup(repository => repository.GetUnreadNotificationCount(It.IsAny<int>())).Returns(0);

        this.service = new DashboardService(
            this.dashboardRepository.Object,
            this.userRepository.Object,
            NullLogger<DashboardService>.Instance);
    }

    [Fact]
    public void GetDashboardData_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(99);

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = fullName, Email = email });

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        this.dashboardRepository.Setup(repository => repository.GetCardsByUser(userId))
            .Returns(new List<Card>
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
            });

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        this.dashboardRepository.Setup(repository => repository.GetCardsByUser(userId)).Returns(Error.Failure());

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        this.dashboardRepository.Setup(repository => repository.GetAccountsByUser(userId))
            .Returns(new List<Account> { new Account { Id = accountId, UserId = userId } });
        this.dashboardRepository.Setup(repository => repository.GetRecentTransactions(accountId, It.IsAny<int>()))
            .Returns(new List<Transaction>
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
            });

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        this.dashboardRepository.Setup(repository => repository.GetAccountsByUser(userId)).Returns(Error.Failure());

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        this.dashboardRepository.Setup(repository => repository.GetUnreadNotificationCount(userId)).Returns(Error.Failure());

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        this.dashboardRepository.Setup(repository => repository.GetAccountsByUser(userId))
            .Returns(new List<Account>
            {
                new Account { Id = accountId1, UserId = userId },
                new Account { Id = accountId2, UserId = userId },
            });

        List<Transaction> transactions1 = Enumerable.Range(1, 8).Select(i => new Transaction
        {
            Id = i,
            AccountId = accountId1,
            Direction = TransactionDirection.In,
            Amount = i * 10,
            Currency = "RON",
            Status = TransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddMinutes(-i),
        }).ToList();
        List<Transaction> transactions2 = Enumerable.Range(9, 5).Select(i => new Transaction
        {
            Id = i,
            AccountId = accountId2,
            Direction = TransactionDirection.Out,
            Amount = i * 10,
            Currency = "RON",
            Status = TransactionStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddMinutes(-i),
        }).ToList();
        this.dashboardRepository.Setup(repository => repository.GetRecentTransactions(accountId1, It.IsAny<int>()))
            .Returns(transactions1);
        this.dashboardRepository.Setup(repository => repository.GetRecentTransactions(accountId2, It.IsAny<int>()))
            .Returns(transactions2);

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
        this.userRepository.Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, FullName = "Ada", Email = "ada@test.com" });
        this.dashboardRepository.Setup(repository => repository.GetUnreadNotificationCount(userId)).Returns(unreadCount);

        // Act
        ErrorOr<DashboardResponse> result = this.service.GetDashboardData(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.UnreadNotificationCount.Should().Be(unreadCount);
    }
}
