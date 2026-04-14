// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Implementations;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Tests.Integration.Infrastructure;
using Dapper;
using Xunit;

namespace BankApp.Server.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="DashboardRepository"/> verifying that
/// aggregate and collection queries return correct, database-backed results.
/// </summary>
[Trait("Category", "Integration")]
public sealed class DashboardRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture;

    public DashboardRepositoryTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.Reset();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private AppDbContext MakeDb() => this.fixture.CreateDbContext();

    private static User MakeUser(string email) => new()
    {
        Email = email,
        PasswordHash = "pw_hash",
        FullName = "Dashboard User",
        PreferredLanguage = "en",
    };

    /// <summary>Inserts a user and returns the persisted entity.</summary>
    private User SeedUser(AppDbContext db, string email = "dash@test.com")
    {
        var da = new UserDataAccess(db);
        da.Create(MakeUser(email));
        return da.FindByEmail(email).Value;
    }

    /// <summary>Inserts an account and returns the persisted entity.</summary>
    private Account SeedAccount(AppDbContext db, int userId)
    {
        db.Query<int>(conn =>
        {
            conn.Execute(
                """
                INSERT INTO Account (UserId, AccountName, IBAN, Currency, Balance, AccountType, Status)
                VALUES (@UserId, 'Main Account', 'RO49AAAA1B31007593840000', 'RON', 5000.00, 'Checking', 'Active')
                """,
                new { UserId = userId });
            return 0;
        });

        return db.Query<Account>(conn =>
            conn.QueryFirst<Account>(
                "SELECT * FROM Account WHERE UserId = @UserId",
                new { UserId = userId })).Value;
    }

    /// <summary>Inserts a card linked to a user and account.</summary>
    private void SeedCard(AppDbContext db, int accountId, int userId)
    {
        db.Query<int>(conn =>
        {
            conn.Execute(
                """
                INSERT INTO Card (AccountId, UserId, CardNumber, CardholderName, ExpiryDate, CVV, CardType, Status)
                VALUES (@AccountId, @UserId, '4111111111111111', 'Dashboard User', '2027-12-31', '123', 'Debit', 'Active')
                """,
                new { AccountId = accountId, UserId = userId });
            return 0;
        });
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
        var db = MakeDb();
        var user = SeedUser(db, "accounts@test.com");
        SeedAccount(db, user.Id);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetAccountsByUser(user.Id);

        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : string.Empty);
        Assert.Single(result.Value);
        Assert.Equal(user.Id, result.Value[0].UserId);
        Assert.Equal("RON", result.Value[0].Currency);
    }

    /// <summary>
    /// GetRecentTransactions with limit=3 should return at most 3 items
    /// even if more exist, in descending CreatedAt order.
    /// </summary>
    [Fact]
    public void GetRecentTransactions_ReturnsAtMostLimitResults()
    {
        var db = MakeDb();
        var user = SeedUser(db, "txlimit@test.com");
        var account = SeedAccount(db, user.Id);
        SeedTransactions(db, account.Id, 5);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetRecentTransactions(account.Id, limit: 3);

        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : string.Empty);
        Assert.Equal(3, result.Value.Count);
    }

    /// <summary>
    /// GetUnreadNotificationCount should return exactly the number of unread notifications.
    /// </summary>
    [Fact]
    public void GetUnreadNotificationCount_ReturnsCorrectCount()
    {
        var db = MakeDb();
        var user = SeedUser(db, "notifcount@test.com");
        SeedNotifications(db, user.Id, 4);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetUnreadNotificationCount(user.Id);

        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : string.Empty);
        Assert.Equal(4, result.Value);
    }

    /// <summary>
    /// GetCardsByUser should return cards associated with the correct user.
    /// </summary>
    [Fact]
    public void GetCardsByUser_ReturnsCardsForGivenUser()
    {
        var db = MakeDb();
        var user = SeedUser(db, "cards@test.com");
        var account = SeedAccount(db, user.Id);
        SeedCard(db, account.Id, user.Id);

        var repo = MakeDashboardRepo(db);
        var result = repo.GetCardsByUser(user.Id);

        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : string.Empty);
        Assert.Single(result.Value);
        Assert.Equal(user.Id, result.Value[0].UserId);
        Assert.Equal("Debit", result.Value[0].CardType);
    }
}
