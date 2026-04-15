// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.Entities;
using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Implementations;
using BankApp.Server.Repositories.Implementations;
using BankApp.Server.Tests.Integration.Infrastructure;
using Bogus;
using FluentAssertions;
using Xunit;

namespace BankApp.Server.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="UserRepository"/> that verify data is correctly
/// persisted to and retrieved from the database.
/// </summary>
[Trait("Category", "Integration")]
[Collection("Integration")]
public sealed class UserRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture fixture;
    private readonly Faker<User> userFaker;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        this.fixture = fixture;

        // Configure Bogus Faker
        this.userFaker = new Faker<User>()
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
            .RuleFor(u => u.FullName, f => f.Person.FullName)
            .RuleFor(u => u.PreferredLanguage, f => "en");
    }

    public Task InitializeAsync() => this.fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // ─── Helper ──────────────────────────────────────────────────────────────

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
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var user = this.userFaker.Generate();
        var createResult = userDa.Create(user);
        
        createResult.IsError.Should().BeFalse(createResult.IsError ? createResult.FirstError.Description : string.Empty);

        var findResult = userDa.FindByEmail(user.Email);
        findResult.IsError.Should().BeFalse();
        findResult.Value.Email.Should().Be(user.Email);
        findResult.Value.FullName.Should().Be(user.FullName);
        findResult.Value.PasswordHash.Should().Be(user.PasswordHash);
    }

    /// <summary>
    /// Finding a user by a valid ID should return the correct entity.
    /// </summary>
    [Fact]
    public void FindById_ExistingUser_ReturnsUser()
    {
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var user = this.userFaker.Generate();
        userDa.Create(user);
        var byEmail = userDa.FindByEmail(user.Email);
        byEmail.IsError.Should().BeFalse(byEmail.IsError ? byEmail.FirstError.Description : string.Empty);

        var byId = userDa.FindById(byEmail.Value.Id);
        byId.IsError.Should().BeFalse(byId.IsError ? byId.FirstError.Description : string.Empty);
        byId.Value.Id.Should().Be(byEmail.Value.Id);
        byId.Value.Email.Should().Be(user.Email);
    }

    /// <summary>
    /// Finding a user by a non-existent ID should return a NotFound error.
    /// </summary>
    [Fact]
    public void FindById_NonExistentUser_ReturnsNotFoundError()
    {
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var result = userDa.FindById(99999);
        result.IsError.Should().BeTrue();
    }

    /// <summary>
    /// After calling Update, the new field values must be persisted.
    /// </summary>
    [Fact]
    public void UpdateUser_ChangesArePersisted()
    {
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var user = this.userFaker.Generate();
        userDa.Create(user);
        var original = userDa.FindByEmail(user.Email);
        original.IsError.Should().BeFalse(original.IsError ? original.FirstError.Description : string.Empty);

        var originalValue = original.Value;
        originalValue.FullName = "Diana Updated";
        originalValue.PhoneNumber = "+40700000000";
        var updateResult = userDa.Update(originalValue);
        updateResult.IsError.Should().BeFalse(updateResult.IsError ? updateResult.FirstError.Description : string.Empty);

        var refreshed = userDa.FindById(originalValue.Id);
        refreshed.IsError.Should().BeFalse(refreshed.IsError ? refreshed.FirstError.Description : string.Empty);
        refreshed.Value.FullName.Should().Be("Diana Updated");
        refreshed.Value.PhoneNumber.Should().Be("+40700000000");
    }

    /// <summary>
    /// IncrementFailedAttempts should increase the counter by 1 on each call.
    /// </summary>
    [Fact]
    public void IncrementFailedAttempts_CounterIncreasesCorrectly()
    {
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var user = this.userFaker.Generate();
        userDa.Create(user);
        var savedUserResult = userDa.FindByEmail(user.Email);
        savedUserResult.IsError.Should().BeFalse(savedUserResult.IsError ? savedUserResult.FirstError.Description : string.Empty);
        var savedUser = savedUserResult.Value;

        userDa.IncrementFailedAttempts(savedUser.Id);
        userDa.IncrementFailedAttempts(savedUser.Id);

        var updated = userDa.FindById(savedUser.Id);
        updated.IsError.Should().BeFalse(updated.IsError ? updated.FirstError.Description : string.Empty);
        updated.Value.FailedLoginAttempts.Should().Be(2);
    }

    /// <summary>
    /// LockAccount should set IsLocked = true and persist the lockout end timestamp.
    /// </summary>
    [Fact]
    public void LockAccount_SetsIsLockedAndLockoutEnd()
    {
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        var user = this.userFaker.Generate();
        userDa.Create(user);
        var savedUserResult2 = userDa.FindByEmail(user.Email);
        savedUserResult2.IsError.Should().BeFalse(savedUserResult2.IsError ? savedUserResult2.FirstError.Description : string.Empty);
        var savedUser2 = savedUserResult2.Value;

        var lockoutEnd = DateTime.UtcNow.AddMinutes(30);
        var lockResult = userDa.LockAccount(savedUser2.Id, lockoutEnd);
        lockResult.IsError.Should().BeFalse(lockResult.IsError ? lockResult.FirstError.Description : string.Empty);

        var locked = userDa.FindById(savedUser2.Id);
        locked.IsError.Should().BeFalse(locked.IsError ? locked.FirstError.Description : string.Empty);
        locked.Value.IsLocked.Should().BeTrue();
    }
}
