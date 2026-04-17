// <copyright file="DashboardRepositoryTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using BankApp.Infrastructure.DataAccess;
using BankApp.Infrastructure.DataAccess.Implementations;
using BankApp.Infrastructure.Repositories.Implementations;
using BankApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;
using Dapper;
using ErrorOr;

namespace BankApp.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="DashboardRepository"/> verifying that
/// aggregate and collection queries return correct, database-backed results.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class DashboardRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardRepositoryTests"/> class.
    /// </summary>
    /// <param name="fixture">Database fixture.</param>
    public DashboardRepositoryTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;
    }

    /// <inheritdoc/>
    public Task InitializeAsync() => this.fixture.ResetAsync();

    /// <inheritdoc/>
    public Task DisposeAsync() => Task.CompletedTask;

    private AppDatabaseContext MakeDatabaseContext() => this.fixture.CreateDatabaseContext();

    private User SeedUser(AppDatabaseContext databaseContext)
    {
        var faker = new Faker();
        var dataAccess = new UserDataAccess(databaseContext);
        var user = new User
        {
            Email = faker.Internet.Email(),
            PasswordHash = faker.Internet.Password(),
            FullName = faker.Person.FullName,
            PreferredLanguage = "en",
        };

        dataAccess.Create(user).IsError.Should().BeFalse();

        ErrorOr<User> findResult = dataAccess.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse(findResult.IsError ? findResult.FirstError.Description : string.Empty);
        return findResult.Value;
    }

    private Account SeedAccount(AppDatabaseContext databaseContext, int userId, string? iban = null)
    {
        var faker = new Faker();
        iban ??= faker.Finance.Iban();

        databaseContext.Query<int>(connection =>
        {
            connection.Execute(
                """
                INSERT INTO Account (UserId, AccountName, IBAN, Currency, Balance, AccountType, Status)
                VALUES (@UserId, 'Main Account', @Iban, 'RON', 5000.00, 'Checking', 'Active')
                """,
                new { UserId = userId, Iban = iban });
            return 0;
        });

        return databaseContext.Query<Account>(connection =>
            connection.QueryFirst<Account>(
                "SELECT * FROM Account WHERE UserId = @UserId AND IBAN = @Iban",
                new { UserId = userId, Iban = iban })).Value;
    }

    private void SeedCard(AppDatabaseContext databaseContext, int accountId, int userId)
    {
        // Use a fixed card number to avoid Bogus generating numbers > VARCHAR(19)
        ErrorOr<int> seedResult = databaseContext.Query<int>(connection =>
        {
            return connection.Execute(
                """
                INSERT INTO Card (AccountId, UserId, CardNumber, CardholderName, ExpiryDate, CVV, CardType, Status)
                VALUES (@AccountId, @UserId, '4111111111111111', 'Test User', '2027-12-31', '123', 'Debit', 'Active')
                """,
                new { AccountId = accountId, UserId = userId });
        });

        seedResult.IsError.Should().BeFalse(seedResult.IsError ? seedResult.FirstError.Description : "SeedCard INSERT failed.");
    }

    private void SeedTransactions(AppDatabaseContext databaseContext, int accountId, int count)
    {
        for (var index = 0; index < count; index++)
        {
            int localIndex = index;
            databaseContext.Query<int>(connection =>
            {
                connection.Execute(
                    """
                    INSERT INTO "Transaction" (AccountId, TransactionRef, Type, Direction, Amount, Currency, BalanceAfter, Status)
                    VALUES (@AccountId, @Ref, 'Transfer', 'In', 100.00, 'RON', 5100.00, 'Completed')
                    """,
                    new { AccountId = accountId, Ref = $"REF-{localIndex}-{Guid.NewGuid():N}" });
                return 0;
            });
        }
    }

    private void SeedNotifications(AppDatabaseContext databaseContext, int userId, int count)
    {
        for (var index = 0; index < count; index++)
        {
            databaseContext.Query<int>(connection =>
            {
                connection.Execute(
                    """
                    INSERT INTO Notification (UserId, Title, Message, Type, Channel, IsRead)
                    VALUES (@UserId, 'Info', 'You have a new notification.', 'Alert', 'Push', 0)
                    """,
                    new { UserId = userId });
                return 0;
            });
        }
    }

    private DashboardRepository MakeDashboardRepository(AppDatabaseContext databaseContext)
    {
        return new DashboardRepository(
            new AccountDataAccess(databaseContext),
            new CardDataAccess(databaseContext),
            new TransactionDataAccess(databaseContext),
            new NotificationDataAccess(databaseContext));
    }

    [Fact]
    public void GetAccountsByUser_WhenUserHasAccounts_ReturnsAllAccounts()
    {
        // Arrange
        using AppDatabaseContext databaseContext = this.MakeDatabaseContext();
        User user = this.SeedUser(databaseContext);
        this.SeedAccount(databaseContext, user.Id);
        var repository = this.MakeDashboardRepository(databaseContext);

        // Act
        var result = repository.GetAccountsByUser(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().ContainSingle();
        result.Value.First().UserId.Should().Be(user.Id);
        result.Value.First().Currency.Should().Be("RON");
    }

    [Fact]
    public void GetRecentTransactions_WhenInsertedMoreThanLimit_ReturnsAtMostLimitItems()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var user = SeedUser(databaseContext);
        var account = SeedAccount(databaseContext, user.Id);
        SeedTransactions(databaseContext, account.Id, 5);
        var repository = MakeDashboardRepository(databaseContext);

        // Act
        var result = repository.GetRecentTransactions(account.Id, limit: 3);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public void GetUnreadNotificationCount_WhenNotificationsExist_ReturnsCorrectCount()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var user = SeedUser(databaseContext);
        SeedNotifications(databaseContext, user.Id, 4);
        var repository = MakeDashboardRepository(databaseContext);

        // Act
        var result = repository.GetUnreadNotificationCount(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().Be(4);
    }

    [Fact]
    public void GetCardsByUser_WhenUserHasCards_ReturnsCardsForThatUser()
    {
        // Arrange
        using AppDatabaseContext databaseContext = this.MakeDatabaseContext();
        User user = this.SeedUser(databaseContext);
        Account account = this.SeedAccount(databaseContext, user.Id);
        this.SeedCard(databaseContext, account.Id, user.Id);
        DashboardRepository repository = this.MakeDashboardRepository(databaseContext);

        // Act
        ErrorOr<List<Card>> result = repository.GetCardsByUser(user.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().ContainSingle();
        result.Value.First().UserId.Should().Be(user.Id);
        result.Value.First().CardType.Should().Be(CardType.Debit);
    }
}
