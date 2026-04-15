// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Implementations;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Tests.Integration.Infrastructure;
using Bogus;
using Dapper;
using FluentAssertions;
using Xunit;

namespace BankApp.Server.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="DashboardRepository"/> verifying that
/// aggregate and collection queries return correct, database-backed results.
/// </summary>
[Trait("Category", "Integration")]
public sealed class DashboardRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture fixture;
    private readonly Faker<User> userFaker;

    public DashboardRepositoryTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;

        this.userFaker = new Faker<User>()
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
            .RuleFor(u => u.FullName, f => f.Person.FullName)
            .RuleFor(u => u.PreferredLanguage, f => "en");
    }

    public Task InitializeAsync() => this.fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private AppDbContext MakeDb() => this.fixture.CreateDbContext();

    /// <summary>Inserts a user and returns the persisted entity.</summary>
    private User SeedUser(AppDbContext db)
    {
        var da = new UserDataAccess(db);
        var user = this.userFaker.Generate();
        var createResult = da.Create(user);
        createResult.IsError.Should().BeFalse(createResult.IsError ? createResult.FirstError.Description : string.Empty);

        var findResult = da.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse(findResult.IsError ? findResult.FirstError.Description : string.Empty);
        return findResult.Value;
    }

    /// <summary>Inserts an account and returns the persisted entity.</summary>
    private Account SeedAccount(AppDbContext db, int userId, string? iban = null)
    {
        var faker = new Faker();
        iban ??= faker.Finance.Iban();

        db.Query<int>(conn =>
        {
            conn.Execute(
                """
                INSERT INTO Account (UserId, AccountName, IBAN, Currency, Balance, AccountType, Status)
                VALUES (@UserId, 'Main Account', @Iban, 'RON', 5000.00, 'Checking', 'Active')
                """,
                new { UserId = userId, Iban = iban });
            return 0;
        });

        return db.Query<Account>(conn =>
            conn.QueryFirst<Account>(
                "SELECT * FROM Account WHERE UserId = @UserId AND IBAN = @Iban",
                new { UserId = userId, Iban = iban })).Value;
    }

    /// <summary>Inserts a card linked to a user and account.</summary>
    private void SeedCard(AppDbContext db, int accountId, int userId)
    {
        // Use a fixed card number to avoid Bogus generating numbers > VARCHAR(19)
        var seedResult = db.Query<int>(conn =>
        {
            return conn.Execute(
                """
                INSERT INTO Card (AccountId, UserId, CardNumber, CardholderName, ExpiryDate, CVV, CardType, Status)
                VALUES (@AccountId, @UserId, '4111111111111111', 'Test User', '2027-12-31', '123', 'Debit', 'Active')
                """,
                new { AccountId = accountId, UserId = userId });
        });

        seedResult.IsError.Should().BeFalse(seedResult.IsError ? seedResult.FirstError.Description : "SeedCard INSERT failed.");
    }

    /// <summary>Inserts N transactions for a given account.</summary>
    private void SeedTransactions(AppDbContext db, int accountId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int localI = i;
            db.Query<int>(conn =>
            {
                conn.Execute(
                    """
                    INSERT INTO "Transaction" (AccountId, TransactionRef, Type, Direction, Amount, Currency, BalanceAfter, Status)
                    VALUES (@AccountId, @Ref, 'Transfer', 'Credit', 100.00, 'RON', 5100.00, 'Completed')
                    """,
                    new { AccountId = accountId, Ref = $"REF-{localI}-{Guid.NewGuid():N}" });
                return 0;
            });
        }
    }

    /// <summary>Inserts N unread notifications for a user.</summary>
    private void SeedNotifications(AppDbContext db, int userId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            db.Query<int>(conn =>
            {
                conn.Execute(
                    """
                    INSERT INTO Notification (UserId, Title, Message, Type, Channel, IsRead)
                    VALUES (@UserId, 'Info', 'You have a new notification.', 'Alert', 'Push', 0)
                    """,
                    new { UserId = userId });
                return 0;
            });
        }
    }

    private DashboardRepository MakeDashboardRepo(AppDbContext db)
    {
        return new DashboardRepository(
            new AccountDataAccess(db),
            new CardDataAccess(db),
            new TransactionDataAccess(db),
            new NotificationDataAccess(db));
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    /// <summary>
    /// GetAccountsByUser should return all accounts belonging to the specified user.
    /// </summary>
    [Fact]
    public void GetAccountsByUser_ReturnsAllAccountsForUser()
    {
        using var db = MakeDb();
        var user = SeedUser(db);
        SeedAccount(db, user.Id);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetAccountsByUser(user.Id);

        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().ContainSingle();
        result.Value[0].UserId.Should().Be(user.Id);
        result.Value[0].Currency.Should().Be("RON");
    }

    /// <summary>
    /// GetRecentTransactions with limit=3 should return at most 3 items
    /// even if more exist, in descending CreatedAt order.
    /// </summary>
    [Fact]
    public void GetRecentTransactions_ReturnsAtMostLimitResults()
    {
        using var db = MakeDb();
        var user = SeedUser(db);
        var account = SeedAccount(db, user.Id);
        SeedTransactions(db, account.Id, 5);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetRecentTransactions(account.Id, limit: 3);

        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().HaveCount(3);
    }

    /// <summary>
    /// GetUnreadNotificationCount should return exactly the number of unread notifications.
    /// </summary>
    [Fact]
    public void GetUnreadNotificationCount_ReturnsCorrectCount()
    {
        using var db = MakeDb();
        var user = SeedUser(db);
        SeedNotifications(db, user.Id, 4);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetUnreadNotificationCount(user.Id);

        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().Be(4);
    }

    /// <summary>
    /// GetCardsByUser should return cards associated with the correct user.
    /// </summary>
    [Fact]
    public void GetCardsByUser_ReturnsCardsForGivenUser()
    {
        using var db = MakeDb();
        var user = SeedUser(db);
        var account = SeedAccount(db, user.Id);
        SeedCard(db, account.Id, user.Id);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetCardsByUser(user.Id);

        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Should().ContainSingle();
        result.Value[0].UserId.Should().Be(user.Id);
        result.Value[0].CardType.Should().Be("Debit");
    }
}
