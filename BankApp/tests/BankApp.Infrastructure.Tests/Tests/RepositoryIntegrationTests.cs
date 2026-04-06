// <copyright file="RepositoryIntegrationTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Infrastructure.DataAccess.Implementations;
using BankApp.Infrastructure.Repositories.Interfaces;

namespace BankApp.Infrastructure.Tests.Tests;

/// <summary>
/// Integration tests that exercise the full Repository layer composed over real DataAccess
/// objects backed by SQLite in-memory.
/// Validates that multi-entity queries (User + Accounts + Cards + Transactions)
/// compose correctly and all navigation data is returned without nulls.
/// </summary>
public class RepositoryIntegrationTests : IDisposable
{
    private readonly DatabaseFixture fixture = new();
    private readonly SqliteDbContext ctx;

    // DataAccess wires
    private readonly UserDataAccess userDa;
    private readonly SessionDataAccess sessionDa;
    private readonly OAuthLinkDataAccess oauthDa;
    private readonly PasswordResetTokenDataAccess tokenDa;
    private readonly NotificationPreferenceDataAccess notifPrefDa;
    private readonly AccountDataAccess accountDa;
    private readonly CardDataAccess cardDa;
    private readonly TransactionDataAccess transactionDa;
    private readonly NotificationDataAccess notificationDa;

    // Repositories under test
    private readonly UserRepository userRepo;
    private readonly AuthRepository authRepo;
    private readonly DashboardRepository dashboardRepo;

    public RepositoryIntegrationTests()
    {
        ctx = fixture.CreateContext();

        userDa = new UserDataAccess(ctx);
        sessionDa = new SessionDataAccess(ctx);
        oauthDa = new OAuthLinkDataAccess(ctx);
        tokenDa = new PasswordResetTokenDataAccess(ctx);
        notifPrefDa = new NotificationPreferenceDataAccess(ctx);
        accountDa = new AccountDataAccess(ctx);
        cardDa = new CardDataAccess(ctx);
        transactionDa = new TransactionDataAccess(ctx);
        notificationDa = new NotificationDataAccess(ctx);

        userRepo = new UserRepository(userDa, sessionDa, oauthDa, notifPrefDa);
        authRepo = new AuthRepository(userDa, sessionDa, oauthDa, tokenDa, notifPrefDa);
        dashboardRepo = new DashboardRepository(accountDa, cardDa, transactionDa, notificationDa);
    }

    public void Dispose() => ctx.Dispose();

    // ── UserRepository ────────────────────────────────────────────────────────

    [Fact]
    public void UserRepository_FindById_ReturnsUserCreatedViaAuthRepository()
    {
        var newUser = new User
        {
            Email = "dave@example.com",
            PasswordHash = "hash_dave",
            FullName = "Dave Test",
            PreferredLanguage = "en",
        };
        authRepo.CreateUser(newUser);
        int id = authRepo.FindUserByEmail("dave@example.com")!.Id;

        var found = userRepo.FindById(id);

        found.Should().NotBeNull();
        found!.Email.Should().Be("dave@example.com");
        found.FullName.Should().Be("Dave Test");
    }

    [Fact]
    public void UserRepository_UpdateUser_PersistsChanges()
    {
        var user = new User
        {
            Email = "eve@example.com",
            PasswordHash = "hash_eve",
            FullName = "Eve Original",
            PreferredLanguage = "en",
        };
        authRepo.CreateUser(user);
        var saved = authRepo.FindUserByEmail("eve@example.com")!;

        saved.FullName = "Eve Updated";
        saved.PhoneNumber = "+40799000000";
        bool updated = userRepo.UpdateUser(saved);

        var result = userRepo.FindById(saved.Id)!;
        updated.Should().BeTrue();
        result.FullName.Should().Be("Eve Updated");
        result.PhoneNumber.Should().Be("+40799000000");
    }

    [Fact]
    public void UserRepository_UpdatePassword_NewHashStoredCorrectly()
    {
        int id = DatabaseFixture.SeedUser(ctx, "frank@example.com", "Frank Test");

        bool updated = userRepo.UpdatePassword(id, "new_hashed_password");
        var result = userRepo.FindById(id)!;

        updated.Should().BeTrue();
        result.PasswordHash.Should().Be("new_hashed_password");
    }

    [Fact]
    public void UserRepository_GetActiveSessions_ReturnsSessionsCreatedViaAuthRepository()
    {
        int id = DatabaseFixture.SeedUser(ctx, "grace@example.com", "Grace Test");
        authRepo.CreateSession(id, "tok_grace_1", null, "Chrome", null);
        authRepo.CreateSession(id, "tok_grace_2", null, "Firefox", null);

        var sessions = userRepo.GetActiveSessions(id);

        sessions.Should().HaveCount(2);
        sessions.Should().AllSatisfy(s => s.UserId.Should().Be(id));
    }

    [Fact]
    public void UserRepository_GetLinkedProviders_ReturnsOAuthLinksForUser()
    {
        int id = DatabaseFixture.SeedUser(ctx, "henry@example.com", "Henry Test");
        userRepo.SaveOAuthLink(id, "Google", "google_uid_001", "henry@gmail.com");
        userRepo.SaveOAuthLink(id, "GitHub", "gh_uid_002", null);

        var links = userRepo.GetLinkedProviders(id);

        links.Should().HaveCount(2);
        links.Should().ContainSingle(l => l.Provider == "Google");
        links.Should().ContainSingle(l => l.Provider == "GitHub");
    }

    // ── DashboardRepository ───────────────────────────────────────────────────

    [Fact]
    public void DashboardRepository_GetAccountsByUser_ReturnsAccountsWithCorrectData()
    {
        int uid = DatabaseFixture.SeedUser(ctx, "iris@example.com", "Iris Test");
        DatabaseFixture.SeedAccount(ctx, uid, "RO49AAAA0000000000000001");
        DatabaseFixture.SeedAccount(ctx, uid, "RO49AAAA0000000000000002");

        var accounts = dashboardRepo.GetAccountsByUser(uid);

        accounts.Should().HaveCount(2);
        accounts.Should().AllSatisfy(a =>
        {
            a.UserId.Should().Be(uid);
            a.Currency.Should().Be("RON");
            a.AccountType.Should().Be("Checking");
            a.Status.Should().Be("Active");
        });
    }

    [Fact]
    public void DashboardRepository_GetCardsByUser_ReturnsCardsLinkedToCorrectUser()
    {
        int uid = DatabaseFixture.SeedUser(ctx, "jake@example.com", "Jake Test");
        int accountId = DatabaseFixture.SeedAccount(ctx, uid, "RO49AAAA0000000000000099");
        DatabaseFixture.SeedCard(ctx, accountId, uid);

        var cards = dashboardRepo.GetCardsByUser(uid);

        cards.Should().ContainSingle();
        cards[0].UserId.Should().Be(uid);
        cards[0].AccountId.Should().Be(accountId);
        cards[0].CardNumber.Should().Be("4111111111111111");
    }

    [Fact]
    public void DashboardRepository_GetRecentTransactions_ReturnsMostRecentFirst()
    {
        int uid = DatabaseFixture.SeedUser(ctx, "kate@example.com", "Kate Test");
        int accountId = DatabaseFixture.SeedAccount(ctx, uid, "RO49AAAA0000000000000088");

        for (int i = 1; i <= 4; i++)
        {
            ctx.ExecuteNonQuery(
                @"INSERT INTO ""Transaction""
                  (AccountId, CardId, TransactionRef, Type, Direction, Amount, Currency, BalanceAfter, Fee, Status, CreatedAt)
                  VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, datetime('now', @p10))",
                new object[] { accountId, DBNull.Value, $"KATE_TXN_{i:00}", "Transfer", "Debit", i * 10m, "RON", 1000m, 0m, "Completed", $"-{5 - i} seconds" });
        }

        var txns = dashboardRepo.GetRecentTransactions(accountId, limit: 3);

        txns.Should().HaveCount(3, "limit=3 must be respected");
        // Most recent first
        for (int i = 0; i < txns.Count - 1; i++)
        {
            txns[i].CreatedAt.Should().BeOnOrAfter(txns[i + 1].CreatedAt);
        }
    }

    [Fact]
    public void DashboardRepository_GetUnreadNotificationCount_ReturnsCorrectCount()
    {
        int uid = DatabaseFixture.SeedUser(ctx, "leo@example.com", "Leo Test");
        ctx.ExecuteNonQuery(
            "INSERT INTO Notification (UserId, Title, Message, Type, Channel, IsRead) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
            new object[] { uid, "A", "Msg", "Info", "Push", 0 });
        ctx.ExecuteNonQuery(
            "INSERT INTO Notification (UserId, Title, Message, Type, Channel, IsRead) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
            new object[] { uid, "B", "Msg", "Info", "Push", 0 });
        ctx.ExecuteNonQuery(
            "INSERT INTO Notification (UserId, Title, Message, Type, Channel, IsRead) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
            new object[] { uid, "C", "Msg", "Info", "Push", 1 });

        int count = dashboardRepo.GetUnreadNotificationCount(uid);

        count.Should().Be(2);
    }

    [Fact]
    public void DashboardRepository_AccountsAndCards_RelateCorrectlyByForeignKey()
    {
        int uid = DatabaseFixture.SeedUser(ctx, "mia@example.com", "Mia Test");
        int accountId = DatabaseFixture.SeedAccount(ctx, uid, "RO49AAAA0000000000000077");
        DatabaseFixture.SeedCard(ctx, accountId, uid);

        var accounts = dashboardRepo.GetAccountsByUser(uid);
        var cards = dashboardRepo.GetCardsByUser(uid);

        accounts.Should().ContainSingle();
        cards.Should().ContainSingle();
        cards[0].AccountId.Should().Be(accounts[0].Id, "card must reference the correct account FK");
    }
}
