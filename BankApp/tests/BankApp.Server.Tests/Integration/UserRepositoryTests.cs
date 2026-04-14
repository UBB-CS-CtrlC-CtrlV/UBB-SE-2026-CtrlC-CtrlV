// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Implementations;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Tests.Integration.Infrastructure;
using Xunit;

namespace BankApp.Server.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="UserRepository"/> that verify data is correctly
/// persisted to and retrieved from the database.
/// </summary>
[Trait("Category", "Integration")]
public sealed class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture fixture;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.Reset();
    }

    // ─── Helper ──────────────────────────────────────────────────────────────

    private static User MakeUser(string email = "alice@example.com") => new()
    {
        Email = email,
        PasswordHash = "hash_abc",
        FullName = "Alice Example",
        PreferredLanguage = "en",
    };

    private (UserRepository repo, AppDbContext db) CreateRepo()
    {
        var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);
        var sessionDa = new SessionDataAccess(db);
        var oauthDa = new OAuthLinkDataAccess(db);
        var notifDa = new NotificationPreferenceDataAccess(db);
        var repo = new UserRepository(userDa, sessionDa, oauthDa, notifDa);
        return (repo, db);
    }

    // ─── Tests ────────────────────────────────────────────────────────────────

    /// <summary>
    /// After creating a user, FindByEmail should return a user with matching fields.
    /// </summary>
    [Fact]
    public void CreateUser_ThenFindByEmail_ReturnsCorrectUser()
    {
        var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var user = MakeUser("bob@example.com");
        var createResult = userDa.Create(user);
        Assert.False(createResult.IsError, createResult.IsError ? createResult.FirstError.Description : string.Empty);

        var findResult = userDa.FindByEmail("bob@example.com");
        Assert.False(findResult.IsError);
        Assert.Equal("bob@example.com", findResult.Value.Email);
        Assert.Equal("Alice Example", findResult.Value.FullName);
        Assert.Equal("hash_abc", findResult.Value.PasswordHash);
    }

    /// <summary>
    /// Finding a user by a valid ID should return the correct entity.
    /// </summary>
    [Fact]
    public void FindById_ExistingUser_ReturnsUser()
    {
        var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        userDa.Create(MakeUser("charlie@example.com"));
        var byEmail = userDa.FindByEmail("charlie@example.com");
        Assert.False(byEmail.IsError);

        var byId = userDa.FindById(byEmail.Value.Id);
        Assert.False(byId.IsError);
        Assert.Equal(byEmail.Value.Id, byId.Value.Id);
        Assert.Equal("charlie@example.com", byId.Value.Email);
    }

    /// <summary>
    /// Finding a user by a non-existent ID should return a NotFound error.
    /// </summary>
    [Fact]
    public void FindById_NonExistentUser_ReturnsNotFoundError()
    {
        var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var result = userDa.FindById(99999);
        Assert.True(result.IsError);
    }

    /// <summary>
    /// After calling Update, the new field values must be persisted.
    /// </summary>
    [Fact]
    public void UpdateUser_ChangesArePersisted()
    {
        var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        userDa.Create(MakeUser("diana@example.com"));
        var original = userDa.FindByEmail("diana@example.com").Value;

        original.FullName = "Diana Updated";
        original.PhoneNumber = "+40700000000";
        var updateResult = userDa.Update(original);
        Assert.False(updateResult.IsError);

        var refreshed = userDa.FindById(original.Id).Value;
        Assert.Equal("Diana Updated", refreshed.FullName);
        Assert.Equal("+40700000000", refreshed.PhoneNumber);
    }

    /// <summary>
    /// IncrementFailedAttempts should increase the counter by 1 on each call.
    /// </summary>
    [Fact]
    public void IncrementFailedAttempts_CounterIncreasesCorrectly()
    {
        var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        userDa.Create(MakeUser("eve@example.com"));
        var user = userDa.FindByEmail("eve@example.com").Value;

        userDa.IncrementFailedAttempts(user.Id);
        userDa.IncrementFailedAttempts(user.Id);

        var updated = userDa.FindById(user.Id).Value;
        Assert.Equal(2, updated.FailedLoginAttempts);
    }

    /// <summary>
    /// LockAccount should set IsLocked = true and persist the lockout end timestamp.
    /// </summary>
    [Fact]
    public void LockAccount_SetsIsLockedAndLockoutEnd()
    {
        var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        userDa.Create(MakeUser("frank@example.com"));
        var user = userDa.FindByEmail("frank@example.com").Value;

        var lockoutEnd = DateTime.UtcNow.AddMinutes(30);
        var lockResult = userDa.LockAccount(user.Id, lockoutEnd);
        Assert.False(lockResult.IsError);

        var locked = userDa.FindById(user.Id).Value;
        Assert.True(locked.IsLocked);
    }
}
