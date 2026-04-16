// <copyright file="ProfileServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
using BankApp.Contracts.DTOs.Profile;
using BankApp.Contracts.Entities;
using BankApp.Contracts.Enums;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Profile;
using BankApp.Server.Services.Security;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace BankApp.Server.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ProfileService"/>.
/// </summary>
public class ProfileServiceTests
{
    private readonly IUserRepository userRepository = Substitute.For<IUserRepository>();
    private readonly IHashService hashService = Substitute.For<IHashService>();
    private readonly ProfileService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileServiceTests"/> class.
    /// </summary>
    public ProfileServiceTests()
    {
        this.service = new ProfileService(
            this.userRepository,
            this.hashService,
            NullLogger<ProfileService>.Instance);
    }

    [Fact]
    public void GetProfile_WhenUserExists_ReturnsProfileInfo()
    {
        // Arrange
        const int userId = 1;
        const string fullName = "Ada Lovelace";
        const string email = "ada@lovelace.com";
        var user = new User { Id = userId, FullName = fullName, Email = email };
        this.userRepository.FindById(userId).Returns(user);

        // Act
        ErrorOr<ProfileInfo> result = this.service.GetProfile(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FullName.Should().Be(fullName);
        result.Value.Email.Should().Be(email);
        result.Value.UserId.Should().Be(userId);
    }

    [Fact]
    public void GetProfile_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<ProfileInfo> result = this.service.GetProfile(userId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void UpdatePersonalInfo_WhenUserIdIsNull_ReturnsValidationError()
    {
        // Arrange
        var request = new UpdateProfileRequest(null, "0712345678", "123 Main St");

        // Act
        ErrorOr<Success> result = this.service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void UpdatePersonalInfo_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        var request = new UpdateProfileRequest(userId, "0712345678", "123 Main St");
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = this.service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void UpdatePersonalInfo_WhenPhoneIsInvalid_ReturnsValidationError()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada" };
        var request = new UpdateProfileRequest(userId, "not-a-phone", "123 Main St");
        this.userRepository.FindById(userId).Returns(user);

        // Act
        ErrorOr<Success> result = this.service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("invalid_phone");
    }

    [Fact]
    public void UpdatePersonalInfo_WhenValid_UpdatesUserAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        const string validPhone = "0712345678";
        const string address = "123 Main St";
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada" };
        var request = new UpdateProfileRequest(userId, validPhone, address);
        this.userRepository.FindById(userId).Returns(user);
        this.userRepository.UpdateUser(Arg.Any<User>()).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Received(1).UpdateUser(Arg.Is<User>(u => u.PhoneNumber == validPhone && u.Address == address));
    }

    [Fact]
    public void ChangePassword_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        var request = new ChangePasswordRequest(userId, "old", "NewPass1!");
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = this.service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void ChangePassword_WhenNewPasswordIsWeak_ReturnsValidationError()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "hash" };
        var request = new ChangePasswordRequest(userId, "old", "weak");
        this.userRepository.FindById(userId).Returns(user);

        // Act
        ErrorOr<Success> result = this.service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("weak_password");
    }

    [Fact]
    public void ChangePassword_WhenCurrentPasswordIsWrong_ReturnsValidationError()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "hash" };
        var request = new ChangePasswordRequest(userId, "wrongpassword", "NewPass1!");
        this.userRepository.FindById(userId).Returns(user);
        this.hashService.Verify("wrongpassword", "hash").Returns((ErrorOr<bool>)false);

        // Act
        ErrorOr<Success> result = this.service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("incorrect_password");
    }

    [Fact]
    public void ChangePassword_WhenValid_UpdatesPasswordAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        const string newPassword = "NewPass1!";
        const string newHash = "newhash";
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "oldhash" };
        var request = new ChangePasswordRequest(userId, "oldpassword", newPassword);
        this.userRepository.FindById(userId).Returns(user);
        this.hashService.Verify("oldpassword", "oldhash").Returns((ErrorOr<bool>)true);
        this.hashService.GetHash(newPassword).Returns((ErrorOr<string>)newHash);
        this.userRepository.UpdatePassword(userId, newHash).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Received(1).UpdatePassword(userId, newHash);
    }

    [Fact]
    public void Enable2FA_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = this.service.Enable2FA(userId, TwoFactorMethod.Email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Enable2FA_WhenUserExists_EnablesTwoFactorAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada" };
        this.userRepository.FindById(userId).Returns(user);
        this.userRepository.UpdateUser(Arg.Any<User>()).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.Enable2FA(userId, TwoFactorMethod.Email);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Received(1).UpdateUser(Arg.Is<User>(u => u.Is2FAEnabled && u.Preferred2FAMethod == "Email"));
    }

    [Fact]
    public void Disable2FA_WhenUserExists_DisablesTwoFactorAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada", Is2FAEnabled = true };
        this.userRepository.FindById(userId).Returns(user);
        this.userRepository.UpdateUser(Arg.Any<User>()).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.Disable2FA(userId);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Received(1).UpdateUser(Arg.Is<User>(u => !u.Is2FAEnabled && u.Preferred2FAMethod == null));
    }

    [Fact]
    public void VerifyPassword_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<bool> result = this.service.VerifyPassword(userId, "anypassword");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordMatches_ReturnsTrue()
    {
        // Arrange
        const int userId = 1;
        const string password = "correctpassword";
        const string hash = "correcthash";
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = hash };
        this.userRepository.FindById(userId).Returns(user);
        this.hashService.Verify(password, hash).Returns((ErrorOr<bool>)true);

        // Act
        ErrorOr<bool> result = this.service.VerifyPassword(userId, password);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        const int userId = 1;
        const string password = "wrongpassword";
        const string hash = "correcthash";
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = hash };
        this.userRepository.FindById(userId).Returns(user);
        this.hashService.Verify(password, hash).Returns((ErrorOr<bool>)false);

        // Act
        ErrorOr<bool> result = this.service.VerifyPassword(userId, password);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void GetNotificationPreferences_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<List<NotificationPreferenceDto>> result = this.service.GetNotificationPreferences(userId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void GetNotificationPreferences_WhenUserExists_ReturnsMappedPreferences()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada" };
        var prefs = new List<NotificationPreference>
        {
            new NotificationPreference { Id = 1, UserId = userId, Category = NotificationType.Payment, EmailEnabled = true },
        };
        this.userRepository.FindById(userId).Returns(user);
        this.userRepository.GetNotificationPreferences(userId).Returns(prefs);

        // Act
        ErrorOr<List<NotificationPreferenceDto>> result = this.service.GetNotificationPreferences(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value[0].EmailEnabled.Should().BeTrue();
    }

    [Fact]
    public void GetActiveSessions_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<List<SessionDto>> result = this.service.GetActiveSessions(userId);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void GetActiveSessions_WhenUserExists_ReturnsSessions()
    {
        // Arrange
        const int userId = 1;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada" };
        var sessions = new List<Session>
        {
            new Session { Id = 1, UserId = userId, Token = "token1" },
        };
        this.userRepository.FindById(userId).Returns(user);
        this.userRepository.GetActiveSessions(userId).Returns(sessions);

        // Act
        ErrorOr<List<SessionDto>> result = this.service.GetActiveSessions(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
    }

    [Fact]
    public void RevokeSession_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        const int userId = 99;
        this.userRepository.FindById(userId).Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = this.service.RevokeSession(userId, 1);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void RevokeSession_WhenUserExists_RevokesSessionAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        const int sessionId = 42;
        var user = new User { Id = userId, Email = "ada@test.com", FullName = "Ada" };
        this.userRepository.FindById(userId).Returns(user);
        this.userRepository.RevokeSession(userId, sessionId).Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.RevokeSession(userId, sessionId);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Received(1).RevokeSession(userId, sessionId);
    }
}