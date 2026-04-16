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
public sealed class UserRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture fixture;
    private readonly Faker<User> userFaker;

    public UserRepositoryTests(DatabaseFixture fixture)
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

    [Fact]
    public void FindByEmail_AfterCreatingUser_ReturnsUserWithMatchingFields()
    {
        // Arrange
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);
        var user = this.userFaker.Generate();
        userDa.Create(user).IsError.Should().BeFalse();

        // Act
        var result = userDa.FindByEmail(user.Email);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Email.Should().Be(user.Email);
        result.Value.FullName.Should().Be(user.FullName);
        result.Value.PasswordHash.Should().Be(user.PasswordHash);
    }

    [Fact]
    public void FindById_WhenUserExists_ReturnsUser()
    {
        // Arrange
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);
        var user = this.userFaker.Generate();
        userDa.Create(user).IsError.Should().BeFalse();
        var byEmail = userDa.FindByEmail(user.Email);
        byEmail.IsError.Should().BeFalse(byEmail.IsError ? byEmail.FirstError.Description : string.Empty);

        // Act
        var result = userDa.FindById(byEmail.Value.Id);

        // Assert
        result.IsError.Should().BeFalse(result.IsError ? result.FirstError.Description : string.Empty);
        result.Value.Id.Should().Be(byEmail.Value.Id);
        result.Value.Email.Should().Be(user.Email);
    }

    [Fact]
    public void FindById_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);

        // Act
        var result = userDa.FindById(99999);

        // Assert
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public void Update_WhenFieldsAreChanged_PersistsChanges()
    {
        // Arrange
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);
        var user = this.userFaker.Generate();
        userDa.Create(user).IsError.Should().BeFalse();
        var savedUser = userDa.FindByEmail(user.Email);
        savedUser.IsError.Should().BeFalse(savedUser.IsError ? savedUser.FirstError.Description : string.Empty);
        var userToUpdate = savedUser.Value;
        userToUpdate.FullName = "Diana Updated";
        userToUpdate.PhoneNumber = "+40700000000";

        // Act
        var updateResult = userDa.Update(userToUpdate);

        // Assert
        updateResult.IsError.Should().BeFalse(updateResult.IsError ? updateResult.FirstError.Description : string.Empty);
        var refreshed = userDa.FindById(userToUpdate.Id);
        refreshed.IsError.Should().BeFalse(refreshed.IsError ? refreshed.FirstError.Description : string.Empty);
        refreshed.Value.FullName.Should().Be("Diana Updated");
        refreshed.Value.PhoneNumber.Should().Be("+40700000000");
    }

    [Fact]
    public void IncrementFailedAttempts_WhenCalledTwice_CounterIncreasesBy2()
    {
        // Arrange
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);
        var user = this.userFaker.Generate();
        userDa.Create(user).IsError.Should().BeFalse();
        var savedUser = userDa.FindByEmail(user.Email);
        savedUser.IsError.Should().BeFalse(savedUser.IsError ? savedUser.FirstError.Description : string.Empty);

        // Act
        userDa.IncrementFailedAttempts(savedUser.Value.Id);
        userDa.IncrementFailedAttempts(savedUser.Value.Id);

        // Assert
        var updated = userDa.FindById(savedUser.Value.Id);
        updated.IsError.Should().BeFalse(updated.IsError ? updated.FirstError.Description : string.Empty);
        updated.Value.FailedLoginAttempts.Should().Be(2);
    }

    [Fact]
    public void LockAccount_WhenUserExists_SetsIsLockedTrue()
    {
        // Arrange
        using var db = this.fixture.CreateDbContext();
        var userDa = new UserDataAccess(db);
        var user = this.userFaker.Generate();
        userDa.Create(user).IsError.Should().BeFalse();
        var savedUser = userDa.FindByEmail(user.Email);
        savedUser.IsError.Should().BeFalse(savedUser.IsError ? savedUser.FirstError.Description : string.Empty);
        var lockoutEnd = DateTime.UtcNow.AddMinutes(30);

        // Act
        var lockResult = userDa.LockAccount(savedUser.Value.Id, lockoutEnd);

        // Assert
        lockResult.IsError.Should().BeFalse(lockResult.IsError ? lockResult.FirstError.Description : string.Empty);
        var locked = userDa.FindById(savedUser.Value.Id);
        locked.IsError.Should().BeFalse(locked.IsError ? locked.FirstError.Description : string.Empty);
        locked.Value.IsLocked.Should().BeTrue();
    }
}
