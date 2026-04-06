// <copyright file="DashboardDataAccessTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Infrastructure.Tests.Tests;

/// <summary>
/// Integration tests for <see cref="AccountDataAccess"/>, <see cref="CardDataAccess"/>,
/// <see cref="TransactionDataAccess"/>, and <see cref="NotificationDataAccess"/>.
/// Verifies that aggregate and relational queries work correctly end-to-end.
/// </summary>
public class DashboardDataAccessTests : IDisposable
{
    private readonly DatabaseFixture fixture = new();
    private readonly SqliteDbContext ctx;
    private readonly AccountDataAccess accountDa;
    private readonly CardDataAccess cardDa;
    private readonly TransactionDataAccess transactionDa;
    private readonly NotificationDataAccess notificationDa;
    private int userId;

    public DashboardDataAccessTests()
    {
        ctx = fixture.CreateContext();
        accountDa = new AccountDataAccess(ctx);
        cardDa = new CardDataAccess(ctx);
        transactionDa = new TransactionDataAccess(ctx);
        notificationDa = new NotificationDataAccess(ctx);
        userId = DatabaseFixture.SeedUser(ctx);
    }

    public void Dispose() => ctx.Dispose();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int SeedAccount(string iban = "RO49AAAA1B31007593840000")
        => DatabaseFixture.SeedAccount(ctx, userId, iban);

    private int SeedCard(int accountId)
        => DatabaseFixture.SeedCard(ctx, accountId, userId);

    private void SeedTransaction(int accountId, string txRef, string createdAtOffset = "0 seconds")
    {
        // SQLite datetime: modifiers like '-2 seconds', '-1 seconds' are valid; for now() no modifier needed.
        string createdAt = createdAtOffset == "0 seconds"
            ? "datetime('now')"
            : $"datetime('now', '-{GetAbsoluteSeconds(createdAtOffset)} seconds')";

        ctx.ExecuteNonQuery(
            $@"INSERT INTO ""Transaction""
              (AccountId, CardId, TransactionRef, Type, Direction, Amount, Currency, BalanceAfter, Fee, Status, CreatedAt)
              VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, {createdAt})",
            new object[] { accountId, DBNull.Value, txRef, "Transfer", "Debit", 100m, "RON", 900m, 0m, "Completed" });
    }

    private static int GetAbsoluteSeconds(string offset)
    {
        // offset examples: "-2 seconds", "-1 seconds"
        return Math.Abs(int.Parse(offset.Split(' ')[0]));
    }

    private void SeedNotification(bool isRead = false)
    {
        ctx.ExecuteNonQuery(
            "INSERT INTO Notification (UserId, Title, Message, Type, Channel, IsRead) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)",
            new object[] { userId, "Test Title", "Test Msg", "Info", "Push", isRead ? 1 : 0 });
    }

    // ── AccountDataAccess ─────────────────────────────────────────────────────

    [Fact]
    public void FindByUserId_ReturnsAllAccountsBelongingToUser()
    {
        SeedAccount("RO49AAAA0000000000000001");
        SeedAccount("RO49AAAA0000000000000002");

        var accounts = accountDa.FindByUserId(userId);

        accounts.Should().HaveCount(2);
        accounts.Should().AllSatisfy(a => a.UserId.Should().Be(userId));
    }

    [Fact]
    public void FindByUserId_WhenOtherUserHasAccounts_DoesNotReturnTheirAccounts()
    {
        int otherUserId = DatabaseFixture.SeedUser(ctx, "other@example.com", "Other User");
        SeedAccount("RO49AAAA0000000000000010");
        DatabaseFixture.SeedAccount(ctx, otherUserId, "RO49AAAA0000000000000020");

        var accounts = accountDa.FindByUserId(userId);

        accounts.Should().ContainSingle();
        accounts[0].UserId.Should().Be(userId);
    }

    [Fact]
    public void FindById_ReturnsCorrectAccountWithAllFields()
    {
        int accountId = SeedAccount("RO49AAAA0000000000009999");

        var account = accountDa.FindById(accountId);

        account.Should().NotBeNull();
        account!.Id.Should().Be(accountId);
        account.IBAN.Should().Be("RO49AAAA0000000000009999");
        account.Currency.Should().Be("RON");
        account.AccountType.Should().Be("Checking");
        account.Status.Should().Be("Active");
    }

    // ── CardDataAccess ────────────────────────────────────────────────────────

    [Fact]
    public void FindByUserId_ReturnsCardsForUser()
    {
        int accountId = SeedAccount();
        SeedCard(accountId);
        SeedCard(accountId);

        var cards = cardDa.FindByUserId(userId);

        cards.Should().HaveCount(2);
        cards.Should().AllSatisfy(c =>
        {
            c.UserId.Should().Be(userId);
            c.AccountId.Should().Be(accountId);
        });
    }

    [Fact]
    public void FindById_ReturnsCardWithAllFields()
    {
        int accountId = SeedAccount();
        int cardId = SeedCard(accountId);

        var card = cardDa.FindById(cardId);

        card.Should().NotBeNull();
        card!.Id.Should().Be(cardId);
        card.CardNumber.Should().Be("4111111111111111");
        card.CardholderName.Should().Be("Test User");
        card.CardType.Should().Be("Debit");
        card.Status.Should().Be("Active");
        card.IsContactlessEnabled.Should().BeTrue();
        card.IsOnlineEnabled.Should().BeTrue();
    }

    // ── TransactionDataAccess ─────────────────────────────────────────────────

    [Fact]
    public void FindRecentByAccountId_ReturnsTransactionsForAccount()
    {
        int accountId = SeedAccount();
        SeedTransaction(accountId, "TXN001");
        SeedTransaction(accountId, "TXN002");

        var txns = transactionDa.FindRecentByAccountId(accountId, 10);

        txns.Should().HaveCount(2);
        txns.Should().AllSatisfy(t => t.AccountId.Should().Be(accountId));
    }

    [Fact]
    public void FindRecentByAccountId_RespectsLimit()
    {
        int accountId = SeedAccount();
        for (int i = 1; i <= 5; i++)
        {
            SeedTransaction(accountId, $"TXN_LIM_{i:00}");
        }

        var txns = transactionDa.FindRecentByAccountId(accountId, 3);

        txns.Should().HaveCount(3);
    }

    [Fact]
    public void FindRecentByAccountId_ReturnsTransactionsOrderedByCreatedAtDescending()
    {
        int accountId = SeedAccount();

        // Use explicit absolute timestamps to guarantee ordering
        ctx.ExecuteNonQuery(
            @"INSERT INTO ""Transaction"" (AccountId, CardId, TransactionRef, Type, Direction, Amount, Currency, BalanceAfter, Fee, Status, CreatedAt)
              VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, '2024-01-01 00:00:01')",
            new object[] { accountId, DBNull.Value, "TXN_OLD", "Transfer", "Debit", 100m, "RON", 900m, 0m, "Completed" });

        ctx.ExecuteNonQuery(
            @"INSERT INTO ""Transaction"" (AccountId, CardId, TransactionRef, Type, Direction, Amount, Currency, BalanceAfter, Fee, Status, CreatedAt)
              VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, '2024-01-01 00:00:02')",
            new object[] { accountId, DBNull.Value, "TXN_MID", "Transfer", "Debit", 100m, "RON", 900m, 0m, "Completed" });

        ctx.ExecuteNonQuery(
            @"INSERT INTO ""Transaction"" (AccountId, CardId, TransactionRef, Type, Direction, Amount, Currency, BalanceAfter, Fee, Status, CreatedAt)
              VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, '2024-01-01 00:00:03')",
            new object[] { accountId, DBNull.Value, "TXN_NEW", "Transfer", "Debit", 100m, "RON", 900m, 0m, "Completed" });

        var txns = transactionDa.FindRecentByAccountId(accountId, 10);

        txns.Should().HaveCount(3);
        txns[0].TransactionRef.Should().Be("TXN_NEW");   // newest first
        txns[2].TransactionRef.Should().Be("TXN_OLD");
    }

    [Fact]
    public void FindRecentByAccountId_DoesNotReturnTransactionsFromOtherAccounts()
    {
        int acc1 = SeedAccount("RO49AAAA0000000000000001");
        int acc2 = DatabaseFixture.SeedAccount(ctx, userId, "RO49AAAA0000000000000002");
        SeedTransaction(acc1, "TXN_ACC1");
        SeedTransaction(acc2, "TXN_ACC2");

        var txns = transactionDa.FindRecentByAccountId(acc1, 10);

        txns.Should().ContainSingle();
        txns[0].TransactionRef.Should().Be("TXN_ACC1");
    }

    // ── NotificationDataAccess ────────────────────────────────────────────────

    [Fact]
    public void CountUnreadByUserId_CountsOnlyUnreadNotifications()
    {
        SeedNotification(isRead: false);
        SeedNotification(isRead: false);
        SeedNotification(isRead: true);

        int count = notificationDa.CountUnreadByUserId(userId);

        count.Should().Be(2);
    }

    [Fact]
    public void CountUnreadByUserId_WhenNoUnreadNotifications_ReturnsZero()
    {
        SeedNotification(isRead: true);

        int count = notificationDa.CountUnreadByUserId(userId);

        count.Should().Be(0);
    }

    [Fact]
    public void FindByUserId_ReturnsAllNotificationsForUser()
    {
        SeedNotification(isRead: false);
        SeedNotification(isRead: true);

        var notifications = notificationDa.FindByUserId(userId);

        notifications.Should().HaveCount(2);
        notifications.Should().AllSatisfy(n => n.UserId.Should().Be(userId));
    }
}
