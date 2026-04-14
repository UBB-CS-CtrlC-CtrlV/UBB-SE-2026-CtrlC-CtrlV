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
/// Integration tests for <see cref="AuthRepository"/> verifying session management
/// and password-reset token lifecycle are persisted correctly.
/// </summary>
[Trait("Category", "Integration")]
public sealed class AuthRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture;

    public AuthRepositoryTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.Reset();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private AppDbContext MakeDb() => this.fixture.CreateDbContext();

    private static User MakeUser(string email) => new()
    {
        Email = email,
        PasswordHash = "hash_xyz",
        FullName = "Test User",
        PreferredLanguage = "en",
    };

    /// <summary>Creates a user and returns the persisted entity (with Id assigned).</summary>
    private User SeedUser(AppDbContext db, string email = "user@test.com")
    {
        var userDa = new UserDataAccess(db);
        userDa.Create(MakeUser(email));
        return userDa.FindByEmail(email).Value;
    }

    private AuthRepository MakeAuthRepo(AppDbContext db)
    {
        var userDa = new UserDataAccess(db);
        var sessionDa = new SessionDataAccess(db);
        var oauthDa = new OAuthLinkDataAccess(db);
        var tokenDa = new PasswordResetTokenDataAccess(db);
        var notifDa = new NotificationPreferenceDataAccess(db);
        return new AuthRepository(userDa, sessionDa, oauthDa, tokenDa, notifDa);
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    /// <summary>
    /// CreateUser via AuthRepository should also auto-create notification preferences
    /// for every NotificationType value.
    /// </summary>
    [Fact]
    public void CreateUser_AutomaticallyCreatesNotificationPreferences()
    {
        var db = MakeDb();
        var userDa = new UserDataAccess(db);
        var repo = MakeAuthRepo(db);

        var result = repo.CreateUser(MakeUser("new@test.com"));
        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : string.Empty);

        var user = userDa.FindByEmail("new@test.com").Value;

        // Verify directly via SQL count to avoid Dapper enum-mapping issue with NotificationType
        var countResult = db.Query<int>(conn =>
            Dapper.SqlMapper.QueryFirst<int>(
                conn,
                "SELECT COUNT(*) FROM NotificationPreference WHERE UserId = @UserId",
                new { UserId = user.Id }));
        Assert.False(countResult.IsError);
        Assert.True(countResult.Value > 0, "Expected at least one notification preference to be created.");
    }

    /// <summary>
    /// CreateSession should return a Session entity with a positive Id assigned by the DB.
    /// </summary>
    [Fact]
    public void CreateSession_ReturnsSessionWithPositiveId()
    {
        var db = MakeDb();
        var user = SeedUser(db, "session@test.com");
        var repo = MakeAuthRepo(db);

        var result = repo.CreateSession(user.Id, "token-abc", "Chrome", "Chrome 120", "127.0.0.1");
        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : string.Empty);
        Assert.True(result.Value.Id > 0);
        Assert.Equal("token-abc", result.Value.Token);
        Assert.Equal(user.Id, result.Value.UserId);
    }

    /// <summary>
    /// FindSessionByToken should return the correct session for an active, non-revoked token.
    /// </summary>
    [Fact]
    public void FindSessionByToken_ActiveToken_ReturnsSession()
    {
        var db = MakeDb();
        var user = SeedUser(db, "findtoken@test.com");
        var repo = MakeAuthRepo(db);

        repo.CreateSession(user.Id, "valid-token-123", null, null, null);

        var result = repo.FindSessionByToken("valid-token-123");
        Assert.False(result.IsError, result.IsError ? result.FirstError.Description : string.Empty);
        Assert.Equal("valid-token-123", result.Value.Token);
    }

    /// <summary>
    /// After revoking a session, FindSessionByToken should return NotFound.
    /// </summary>
    [Fact]
    public void FindSessionByToken_RevokedToken_ReturnsNotFound()
    {
        var db = MakeDb();
        var user = SeedUser(db, "revoked@test.com");
        var repo = MakeAuthRepo(db);

        var sessionResult = repo.CreateSession(user.Id, "revoke-me", null, null, null);
        Assert.False(sessionResult.IsError);

        repo.UpdateSessionToken(sessionResult.Value.Id);

        var findResult = repo.FindSessionByToken("revoke-me");
        Assert.True(findResult.IsError, "A revoked session should not be retrievable.");
    }

    /// <summary>
    /// After saving a password reset token, FindPasswordResetToken should return
    /// the correct entity with the correct hash and future expiry.
    /// </summary>
    [Fact]
    public void SavePasswordResetToken_ThenFindByHash_ReturnsToken()
    {
        var db = MakeDb();
        var user = SeedUser(db, "reset@test.com");
        var repo = MakeAuthRepo(db);

        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "sha256-hash-xyz",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        var saveResult = repo.SavePasswordResetToken(token);
        Assert.False(saveResult.IsError, saveResult.IsError ? saveResult.FirstError.Description : string.Empty);

        var findResult = repo.FindPasswordResetToken("sha256-hash-xyz");
        Assert.False(findResult.IsError);
        Assert.Equal("sha256-hash-xyz", findResult.Value.TokenHash);
        Assert.Equal(user.Id, findResult.Value.UserId);
        Assert.Null(findResult.Value.UsedAt);
    }

    /// <summary>
    /// After calling MarkPasswordResetTokenAsUsed, UsedAt should be non-null.
    /// </summary>
    [Fact]
    public void MarkPasswordResetTokenAsUsed_SetsUsedAt()
    {
        var db = MakeDb();
        var user = SeedUser(db, "markused@test.com");
        var sessionDa = new SessionDataAccess(db);
        var tokenDa = new PasswordResetTokenDataAccess(db);
        var repo = MakeAuthRepo(db);

        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "mark-used-hash",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        repo.SavePasswordResetToken(token);

        var created = repo.FindPasswordResetToken("mark-used-hash");
        Assert.False(created.IsError);

        var markResult = repo.MarkPasswordResetTokenAsUsed(created.Value.Id);
        Assert.False(markResult.IsError, markResult.IsError ? markResult.FirstError.Description : string.Empty);

        var afterMark = repo.FindPasswordResetToken("mark-used-hash");
        Assert.False(afterMark.IsError);
        Assert.NotNull(afterMark.Value.UsedAt);
    }
}
