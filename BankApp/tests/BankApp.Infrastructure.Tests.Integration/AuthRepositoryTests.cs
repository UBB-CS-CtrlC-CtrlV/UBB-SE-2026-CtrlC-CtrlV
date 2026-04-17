// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Domain.Entities;
using BankApp.Infrastructure.DataAccess;
using BankApp.Infrastructure.DataAccess.Implementations;
using BankApp.Infrastructure.Repositories.Implementations;
using BankApp.Infrastructure.Tests.Integration.Infrastructure;
using Bogus;
using Dapper;
using FluentAssertions;
using Xunit;

namespace BankApp.Infrastructure.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="AuthRepository"/> verifying session management
/// and password-reset token lifecycle are persisted correctly.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class AuthRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture fixture;
    private readonly Faker<User> userFaker;

    public AuthRepositoryTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;

        this.userFaker = new Faker<User>()
            .RuleFor(user => user.Email, faker => faker.Internet.Email())
            .RuleFor(user => user.PasswordHash, faker => faker.Internet.Password())
            .RuleFor(user => user.FullName, faker => faker.Person.FullName)
            .RuleFor(user => user.PreferredLanguage, _ => "en");
    }

    public Task InitializeAsync() => this.fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private AppDatabaseContext MakeDatabaseContext() => this.fixture.CreateDatabaseContext();

    private User SeedUser(AppDatabaseContext databaseContext)
    {
        var userDataAccess = new UserDataAccess(databaseContext);
        var user = this.userFaker.Generate();
        userDataAccess.Create(user).IsError.Should().BeFalse();

        var findResult = userDataAccess.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse(findResult.IsError ? findResult.FirstError.Description : string.Empty);
        return findResult.Value;
    }

    private AuthRepository MakeAuthenticationRepository(AppDatabaseContext databaseContext)
    {
        var userDataAccess = new UserDataAccess(databaseContext);
        var sessionDataAccess = new SessionDataAccess(databaseContext);
        var oauthDataAccess = new OAuthLinkDataAccess(databaseContext);
        var tokenDataAccess = new PasswordResetTokenDataAccess(databaseContext);
        var notificationPreferenceDataAccess = new NotificationPreferenceDataAccess(databaseContext);
        return new AuthRepository(userDataAccess, sessionDataAccess, oauthDataAccess, tokenDataAccess, notificationPreferenceDataAccess);
    }

    [Fact]
    public void CreateUser_WhenCalled_AutomaticallyCreatesNotificationPreferences()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var userDataAccess = new UserDataAccess(databaseContext);
        var repository = MakeAuthenticationRepository(databaseContext);
        var newUser = this.userFaker.Generate();

        // Act
        var result = repository.CreateUser(newUser);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        var user = userDataAccess.FindByEmail(newUser.Email).Value;
        var countResult = databaseContext.Query<int>(connection =>
            SqlMapper.QueryFirst<int>(
                connection,
                "SELECT COUNT(*) FROM NotificationPreference WHERE UserId = @UserId",
                new { UserId = user.Id }));
        countResult.IsError.Should().BeFalse();
        countResult.Value.Should().BeGreaterThan(0, "Expected at least one notification preference to be created.");
    }

    [Fact]
    public void CreateSession_WhenUserExists_ReturnsSessionWithPositiveId()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var user = SeedUser(databaseContext);
        var repository = MakeAuthenticationRepository(databaseContext);

        // Act
        var result = repository.CreateSession(user.Id, "token-abc", "Chrome", "Chrome 120", "127.0.0.1");

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().BeGreaterThan(0);
        result.Value.Token.Should().Be("token-abc");
        result.Value.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void FindSessionByToken_WhenTokenIsActive_ReturnsSession()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var user = SeedUser(databaseContext);
        var repository = MakeAuthenticationRepository(databaseContext);
        repository.CreateSession(user.Id, "valid-token-123", null, null, null);

        // Act
        var result = repository.FindSessionByToken("valid-token-123");

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Token.Should().Be("valid-token-123");
    }

    [Fact]
    public void FindSessionByToken_WhenTokenIsRevoked_ReturnsNotFound()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var user = SeedUser(databaseContext);
        var repository = MakeAuthenticationRepository(databaseContext);
        var sessionResult = repository.CreateSession(user.Id, "revoke-me", null, null, null);
        sessionResult.IsError.Should().BeFalse();
        repository.UpdateSessionToken(sessionResult.Value.Id);

        // Act
        var result = repository.FindSessionByToken("revoke-me");

        // Assert
        result.IsError.Should().BeTrue("A revoked session should not be retrievable.");
    }

    [Fact]
    public void FindPasswordResetToken_AfterSavingToken_ReturnsPersistedToken()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var user = SeedUser(databaseContext);
        var repository = MakeAuthenticationRepository(databaseContext);
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "sha256-hash-xyz",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        repository.SavePasswordResetToken(token).IsError.Should().BeFalse();

        // Act
        var result = repository.FindPasswordResetToken("sha256-hash-xyz");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.TokenHash.Should().Be("sha256-hash-xyz");
        result.Value.UserId.Should().Be(user.Id);
        result.Value.UsedAt.Should().BeNull();
    }

    [Fact]
    public void MarkPasswordResetTokenAsUsed_WhenTokenExists_SetsUsedAtTimestamp()
    {
        // Arrange
        using var databaseContext = MakeDatabaseContext();
        var user = SeedUser(databaseContext);
        var repository = MakeAuthenticationRepository(databaseContext);
        var token = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = "mark-used-hash",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
        };
        repository.SavePasswordResetToken(token).IsError.Should().BeFalse();
        var created = repository.FindPasswordResetToken("mark-used-hash");
        created.IsError.Should().BeFalse();

        // Act
        var markResult = repository.MarkPasswordResetTokenAsUsed(created.Value.Id);

        // Assert
        markResult.IsError.Should().BeFalse(markResult.IsError ? markResult.FirstError.Description : string.Empty);
        var afterMark = repository.FindPasswordResetToken("mark-used-hash");
        afterMark.IsError.Should().BeFalse();
        afterMark.Value.UsedAt.Should().NotBeNull();
    }
}