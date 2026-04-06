// <copyright file="UserDataAccessTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Infrastructure.Tests.Tests;

/// <summary>
/// Integration tests that verify <see cref="UserDataAccess"/> communicates correctly
/// with the database (SQLite in-memory).
/// Each test gets a fresh isolated database – no shared state.
/// </summary>
public class UserDataAccessTests : IDisposable
{
    private readonly DatabaseFixture fixture = new();
    private readonly SqliteDbContext ctx;
    private readonly UserDataAccess sut;

    public UserDataAccessTests()
    {
        ctx = fixture.CreateContext();
        sut = new UserDataAccess(ctx);
    }

    public void Dispose() => ctx.Dispose();

    // ── Helpers ──────────────────────────────────────────────────────────────

    private User MakeUser(string email = "alice@example.com") => new()
    {
        Email = email,
        PasswordHash = "bcrypt_hash_value",
        FullName = "Alice Test",
        PreferredLanguage = "en",
        PhoneNumber = "+40700000000",
        Is2FAEnabled = false,
    };

    // ── FindByEmail ───────────────────────────────────────────────────────────

    [Fact]
    public void FindByEmail_WhenUserExists_ReturnsMatchingUser()
    {
        var user = MakeUser();
        sut.Create(user);

        var result = sut.FindByEmail(user.Email);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.FullName.Should().Be(user.FullName);
    }

    [Fact]
    public void FindByEmail_WhenUserDoesNotExist_ReturnsNull()
    {
        var result = sut.FindByEmail("nonexistent@example.com");

        result.Should().BeNull();
    }

    // ── FindById ──────────────────────────────────────────────────────────────

    [Fact]
    public void FindById_WhenUserExists_ReturnsCorrectUser()
    {
        var user = MakeUser();
        sut.Create(user);
        int id = sut.FindByEmail(user.Email)!.Id;

        var result = sut.FindById(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public void FindById_WhenUserDoesNotExist_ReturnsNull()
    {
        var result = sut.FindById(9999);

        result.Should().BeNull();
    }

    // ── Create → FindById round-trip ──────────────────────────────────────────

    [Fact]
    public void Create_ThenFindById_AllFieldsPersistedCorrectly()
    {
        var user = new User
        {
            Email = "bob@example.com",
            PasswordHash = "hash_abc",
            FullName = "Bob Builder",
            PhoneNumber = "+40722000000",
            Nationality = "RO",
            Address = "Street 1",
            PreferredLanguage = "ro",
            Is2FAEnabled = true,
            Preferred2FAMethod = "Email",
        };

        bool created = sut.Create(user);
        var saved = sut.FindByEmail(user.Email)!;

        created.Should().BeTrue();
        saved.Email.Should().Be(user.Email);
        saved.FullName.Should().Be(user.FullName);
        saved.PhoneNumber.Should().Be(user.PhoneNumber);
        saved.Nationality.Should().Be(user.Nationality);
        saved.PreferredLanguage.Should().Be(user.PreferredLanguage);
        saved.Is2FAEnabled.Should().BeTrue();
        saved.Preferred2FAMethod.Should().Be(user.Preferred2FAMethod);
        saved.IsLocked.Should().BeFalse();
        saved.FailedLoginAttempts.Should().Be(0);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public void Update_ChangesAreSavedToDatabase()
    {
        var user = MakeUser();
        sut.Create(user);
        var saved = sut.FindByEmail(user.Email)!;

        saved.FullName = "Alice Updated";
        saved.PhoneNumber = "+40711111111";
        saved.PreferredLanguage = "ro";
        bool updated = sut.Update(saved);

        var result = sut.FindById(saved.Id)!;
        updated.Should().BeTrue();
        result.FullName.Should().Be("Alice Updated");
        result.PhoneNumber.Should().Be("+40711111111");
        result.PreferredLanguage.Should().Be("ro");
    }

    [Fact]
    public void Update_NonExistentUser_ReturnsFalse()
    {
        var ghost = new User
        {
            Id = 99999,
            Email = "ghost@example.com",
            PasswordHash = "x",
            FullName = "Ghost",
            PreferredLanguage = "en",
        };

        bool result = sut.Update(ghost);

        result.Should().BeFalse();
    }

    // ── UpdatePassword ────────────────────────────────────────────────────────

    [Fact]
    public void UpdatePassword_NewHashIsPersistedAndReadBack()
    {
        var user = MakeUser();
        sut.Create(user);
        int id = sut.FindByEmail(user.Email)!.Id;

        bool updated = sut.UpdatePassword(id, "new_secure_hash");
        var result = sut.FindById(id)!;

        updated.Should().BeTrue();
        result.PasswordHash.Should().Be("new_secure_hash");
    }

    // ── IncrementFailedAttempts ───────────────────────────────────────────────

    [Fact]
    public void IncrementFailedAttempts_CounterIncreasesEachCall()
    {
        sut.Create(MakeUser());
        int id = sut.FindByEmail("alice@example.com")!.Id;

        sut.IncrementFailedAttempts(id);
        sut.IncrementFailedAttempts(id);
        sut.IncrementFailedAttempts(id);

        sut.FindById(id)!.FailedLoginAttempts.Should().Be(3);
    }

    // ── ResetFailedAttempts ───────────────────────────────────────────────────

    [Fact]
    public void ResetFailedAttempts_SetsCounterToZero()
    {
        sut.Create(MakeUser());
        int id = sut.FindByEmail("alice@example.com")!.Id;
        sut.IncrementFailedAttempts(id);
        sut.IncrementFailedAttempts(id);

        sut.ResetFailedAttempts(id);

        sut.FindById(id)!.FailedLoginAttempts.Should().Be(0);
    }

    // ── LockAccount ───────────────────────────────────────────────────────────

    [Fact]
    public void LockAccount_SetsIsLockedTrueAndPersistsLockoutEnd()
    {
        sut.Create(MakeUser());
        int id = sut.FindByEmail("alice@example.com")!.Id;
        var lockoutEnd = DateTime.UtcNow.AddHours(1);

        sut.LockAccount(id, lockoutEnd);
        var result = sut.FindById(id)!;

        result.IsLocked.Should().BeTrue();
        result.LockoutEnd.Should().BeCloseTo(lockoutEnd, TimeSpan.FromSeconds(2));
    }
}
