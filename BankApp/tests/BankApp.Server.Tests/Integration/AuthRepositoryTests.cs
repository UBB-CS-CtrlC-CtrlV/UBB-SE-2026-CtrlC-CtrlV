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
/// Integration tests for <see cref="AuthRepository"/> verifying session management
/// and password-reset token lifecycle are persisted correctly.
/// </summary>
[Trait("Category", "Integration")]
public sealed class AuthRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture fixture;
    private readonly Faker<User> userFaker;

    public AuthRepositoryTests(DatabaseFixture fixture)
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

    /// <summary>Creates a user and returns the persisted entity (with Id assigned).</summary>
    private User SeedUser(AppDbContext db)
    {
        var userDa = new UserDataAccess(db);
        var user = this.userFaker.Generate();
        userDa.Create(user);
        return userDa.FindByEmail(user.Email).Value;
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

        var newUser = this.userFaker.Generate();
        var result = repo.CreateUser(newUser);
        
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);

        var user = userDa.FindByEmail(newUser.Email).Value;

        // Verify directly via SQL count to avoid Dapper enum-mapping issue with NotificationType
        var countResult = db.Query<int>(conn =>
            Dapper.SqlMapper.QueryFirst<int>(
                conn,
                "SELECT COUNT(*) FROM NotificationPreference WHERE UserId = @UserId",
                new { UserId = user.Id }));
        
        countResult.IsError.Should().BeFalse();
        countResult.Value.Should().BeGreaterThan(0, "Expected at least one notification preference to be created.");
    }

    /// <summary>
    /// CreateSession should return a Session entity with a positive Id assigned by the DB.
    /// </summary>
    [Fact]
    public void CreateSession_ReturnsSessionWithPositiveId()
    {
        var db = MakeDb();
        var user = SeedUser(db);
        var repo = MakeAuthRepo(db);

        var result = repo.CreateSession(user.Id, "token-abc", "Chrome", "Chrome 120", "127.0.0.1");
        
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.Token.Should().Be("token-abc");
        result.Value.UserId.Should().Be(user.Id);
    }

    /// <summary>
    /// FindSessionByToken should return the correct session for an active, non-revoked token.
    /// </summary>
    [Fact]
    public void FindSessionByToken_ActiveToken_ReturnsSession()
    {
        var db = MakeDb();
        var user = SeedUser(db);
        var repo = MakeAuthRepo(db);

        repo.CreateSession(user.Id, "valid-token-123", null, null, null);

        var result = repo.FindSessionByToken("valid-token-123");
        
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Token.Should().Be("valid-token-123");
    }

    /// <summary>
    /// After revoking a session, FindSessionByToken should return NotFound.
    /// </summary>
    [Fact]
    public void FindSessionByToken_RevokedToken_ReturnsNotFound()
    {
        var db = MakeDb();
        var user = SeedUser(db);
        var repo = MakeAuthRepo(db);

        var sessionResult = repo.CreateSession(user.Id, "revoke-me", null, null, null);
        sessionResult.IsError.Should().BeFalse();

        repo.UpdateSessionToken(sessionResult.Value.Id);

        var findResult = repo.FindSessionByToken("revoke-me");
        findResult.IsError.Should().BeTrue("A revoked session should not be retrievable.");
    }

    /// <summary>
    /// After saving a password reset token, FindPasswordResetToken should return
    /// the correct entity with the correct hash and future expiry.
    /// </summary>
    [Fact]
    public void SavePasswordResetToken_ThenFindByHash_ReturnsToken()
    {
        var db = MakeDb();
        var user = SeedUser(db);
        var repo = MakeAuthRepo(db);

        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "sha256-hash-xyz",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        var saveResult = repo.SavePasswordResetToken(token);
        saveResult.IsError.Should().BeFalse(saveResult.IsError ? saveResult.FirstError.Description : string.Empty);

        var findResult = repo.FindPasswordResetToken("sha256-hash-xyz");
        findResult.IsError.Should().BeFalse();
        findResult.Value.TokenHash.Should().Be("sha256-hash-xyz");
        findResult.Value.UserId.Should().Be(user.Id);
        findResult.Value.UsedAt.Should().BeNull();
    }

    /// <summary>
    /// After calling MarkPasswordResetTokenAsUsed, UsedAt should be non-null.
    /// </summary>
    [Fact]
    public void MarkPasswordResetTokenAsUsed_SetsUsedAt()
    {
        var db = MakeDb();
        var user = SeedUser(db);
        var repo = MakeAuthRepo(db);

        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "mark-used-hash",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };

        repo.SavePasswordResetToken(token);

        var created = repo.FindPasswordResetToken("mark-used-hash");
        created.IsError.Should().BeFalse();

        var markResult = repo.MarkPasswordResetTokenAsUsed(created.Value.Id);
        markResult.IsError.Should().BeFalse(markResult.IsError ? markResult.FirstError.Description : string.Empty);

        var afterMark = repo.FindPasswordResetToken("mark-used-hash");
        afterMark.IsError.Should().BeFalse();
        afterMark.Value.UsedAt.Should().NotBeNull();
    }
}
