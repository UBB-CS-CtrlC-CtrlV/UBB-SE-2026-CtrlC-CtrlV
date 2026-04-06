// <copyright file="AuthDataAccessTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Infrastructure.DataAccess.Implementations;

namespace BankApp.Infrastructure.Tests.Tests;

/// <summary>
/// Integration tests for <see cref="PasswordResetTokenDataAccess"/> and
/// <see cref="SessionDataAccess"/>, and the full <see cref="AuthRepository"/> flow.
/// Verifies token/session persistence, expiry logic, and revocation – all via SQLite.
/// </summary>
public class AuthDataAccessTests : IDisposable
{
    private readonly DatabaseFixture fixture = new();
    private readonly SqliteDbContext ctx;
    private readonly PasswordResetTokenDataAccess tokenDa;
    private readonly SessionDataAccess sessionDa;
    private int userId;

    public AuthDataAccessTests()
    {
        ctx = fixture.CreateContext();
        tokenDa = new PasswordResetTokenDataAccess(ctx);
        sessionDa = new SessionDataAccess(ctx);
        userId = DatabaseFixture.SeedUser(ctx);
    }

    public void Dispose() => ctx.Dispose();

    // ── PasswordResetToken ────────────────────────────────────────────────────

    [Fact]
    public void CreateToken_ThenFindByHash_ReturnsCorrectToken()
    {
        var expiresAt = DateTime.UtcNow.AddHours(1);
        const string hash = "secure_hash_abc123";

        tokenDa.Create(userId, hash, expiresAt);
        var result = tokenDa.FindByToken(hash);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.TokenHash.Should().Be(hash);
        result.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(2));
        result.UsedAt.Should().BeNull();
    }

    [Fact]
    public void FindByToken_WhenHashDoesNotExist_ReturnsNull()
    {
        var result = tokenDa.FindByToken("nonexistent_hash");

        result.Should().BeNull();
    }

    [Fact]
    public void MarkAsUsed_SetsUsedAtTimestamp()
    {
        var created = tokenDa.Create(userId, "hash_xyz", DateTime.UtcNow.AddHours(1));

        tokenDa.MarkAsUsed(created.Id);
        var result = tokenDa.FindByToken("hash_xyz");

        result!.UsedAt.Should().NotBeNull();
        result.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DeleteExpired_RemovesOnlyExpiredTokens()
    {
        tokenDa.Create(userId, "expired_hash", DateTime.UtcNow.AddHours(-1));
        tokenDa.Create(userId, "valid_hash", DateTime.UtcNow.AddHours(1));

        tokenDa.DeleteExpired();

        tokenDa.FindByToken("expired_hash").Should().BeNull();
        tokenDa.FindByToken("valid_hash").Should().NotBeNull();
    }

    [Fact]
    public void MultipleTokens_EachFoundByOwnHash()
    {
        tokenDa.Create(userId, "hash_A", DateTime.UtcNow.AddMinutes(30));
        tokenDa.Create(userId, "hash_B", DateTime.UtcNow.AddMinutes(60));

        tokenDa.FindByToken("hash_A").Should().NotBeNull();
        tokenDa.FindByToken("hash_B").Should().NotBeNull();
    }

    // ── Session ───────────────────────────────────────────────────────────────

    [Fact]
    public void CreateSession_ThenFindByToken_ReturnsActiveSession()
    {
        const string token = "session_token_001";

        var session = sessionDa.Create(userId, token, "Windows 11", "Chrome", "192.168.1.1");
        var found = sessionDa.FindByToken(token);

        found.Should().NotBeNull();
        found!.UserId.Should().Be(userId);
        found.Token.Should().Be(token);
        found.DeviceInfo.Should().Be("Windows 11");
        found.Browser.Should().Be("Chrome");
        found.IpAddress.Should().Be("192.168.1.1");
        found.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void CreateSession_WithNullOptionalFields_Succeeds()
    {
        var session = sessionDa.Create(userId, "tok_minimal", null, null, null);

        session.Should().NotBeNull();
        session.DeviceInfo.Should().BeNull();
        session.Browser.Should().BeNull();
        session.IpAddress.Should().BeNull();
    }

    [Fact]
    public void RevokeSession_MakesTokenInvisibleToFindByToken()
    {
        const string token = "rev_token";
        var session = sessionDa.Create(userId, token, null, null, null);

        sessionDa.Revoke(session.Id);
        var found = sessionDa.FindByToken(token);

        found.Should().BeNull("revoked sessions must not be returned");
    }

    [Fact]
    public void RevokeAll_RevokesAllSessionsForUser()
    {
        sessionDa.Create(userId, "tok_1", null, null, null);
        sessionDa.Create(userId, "tok_2", null, null, null);
        sessionDa.Create(userId, "tok_3", null, null, null);

        sessionDa.RevokeAll(userId);
        var activeSessions = sessionDa.FindByUserId(userId);

        activeSessions.Should().BeEmpty("all sessions were revoked");
    }

    [Fact]
    public void FindByUserId_ReturnsOnlyActiveNonExpiredSessions()
    {
        sessionDa.Create(userId, "active_tok", null, null, null);

        var sessions = sessionDa.FindByUserId(userId);

        sessions.Should().ContainSingle();
        sessions[0].Token.Should().Be("active_tok");
        sessions[0].IsRevoked.Should().BeFalse();
    }

    // ── AuthRepository full flow ──────────────────────────────────────────────

    [Fact]
    public void AuthRepository_CreateUser_AlsoCreatesNotificationPreferences()
    {
        var userDa = new UserDataAccess(ctx);
        var notifDa = new NotificationPreferenceDataAccess(ctx);
        var oauthDa = new OAuthLinkDataAccess(ctx);

        var repo = new AuthRepository(userDa, sessionDa, oauthDa, tokenDa, notifDa);

        var newUser = new User
        {
            Email = "carol@example.com",
            PasswordHash = "hash_carol",
            FullName = "Carol Test",
            PreferredLanguage = "en",
        };

        bool created = repo.CreateUser(newUser);
        var createdUser = repo.FindUserByEmail("carol@example.com");
        var prefs = notifDa.FindByUserId(createdUser!.Id);

        created.Should().BeTrue();
        prefs.Should().NotBeEmpty("CreateUser must seed notification preferences for every NotificationType");
    }

    [Fact]
    public void AuthRepository_SavePasswordResetToken_CanBeFoundByHash()
    {
        var userDa = new UserDataAccess(ctx);
        var notifDa = new NotificationPreferenceDataAccess(ctx);
        var oauthDa = new OAuthLinkDataAccess(ctx);
        var repo = new AuthRepository(userDa, sessionDa, oauthDa, tokenDa, notifDa);

        var token = new PasswordResetToken
        {
            UserId = userId,
            TokenHash = "repo_hash_001",
            ExpiresAt = DateTime.UtcNow.AddHours(2),
        };

        repo.SavePasswordResetToken(token);
        var found = repo.FindPasswordResetToken("repo_hash_001");

        found.Should().NotBeNull();
        found!.TokenHash.Should().Be("repo_hash_001");
    }
}
