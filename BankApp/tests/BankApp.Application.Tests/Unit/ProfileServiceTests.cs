// <copyright file="ProfileServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
using BankApp.Application.DTOs.Profile;
using BankApp.Domain.Entities;
using BankApp.Domain.Enums;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Profile;
using BankApp.Application.Services.Security;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BankApp.Application.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="ProfileService"/>.
/// </summary>
public class ProfileServiceTests
{
    private readonly Mock<IUserRepository> userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
    private readonly Mock<IHashService> hashService = new Mock<IHashService>(MockBehavior.Strict);
    private readonly ProfileService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileServiceTests"/> class.
    /// </summary>
    public ProfileServiceTests()
    {
        this.service = new ProfileService(
            this.userRepository.Object,
            this.hashService.Object,
            NullLogger<ProfileService>.Instance);
    }

    [Fact]
    public void GetProfile_WhenUserExists_ReturnsProfileInfo()
    {
        // Arrange
        const int userId = 1;
        const string fullName = "Ada Lovelace";
        const string email = "ada@lovelace.com";
        DateTime dateOfBirth = new DateTime(1815, 12, 10);
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User
            {
                Id = userId,
                FullName = fullName,
                Email = email,
                DateOfBirth = dateOfBirth,
                PreferredLanguage = "ro",
            });

        // Act
        ErrorOr<ProfileInfo> result = this.service.GetProfile(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.FullName.Should().Be(fullName);
        result.Value.Email.Should().Be(email);
        result.Value.UserId.Should().Be(userId);
        result.Value.DateOfBirth.Should().Be(dateOfBirth);
        result.Value.PreferredLanguage.Should().Be("ro");
    }

    [Fact]
    public void GetProfile_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<ProfileInfo> result = this.service.GetProfile(99);

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
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());
        var request = new UpdateProfileRequest(99, "0712345678", "123 Main St");

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
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        var request = new UpdateProfileRequest(userId, "not-a-phone", "123 Main St");

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
        const string fullName = "Ada Lovelace";
        const string validPhone = "0712345678";
        const string address = "123 Main St";
        const string nationality = "Romanian";
        const string preferredLanguage = "ro";
        DateTime dateOfBirth = new DateTime(1815, 12, 10);
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        this.userRepository
            .Setup(repository => repository.UpdateUser(It.IsAny<User>()))
            .Returns(Result.Success);
        var request = new UpdateProfileRequest(userId, validPhone, address)
        {
            FullName = fullName,
            DateOfBirth = dateOfBirth,
            Nationality = nationality,
            PreferredLanguage = preferredLanguage,
        };

        // Act
        ErrorOr<Success> result = this.service.UpdatePersonalInfo(request);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Verify(
            repository => repository.UpdateUser(It.Is<User>(user =>
                user.FullName == fullName &&
                user.PhoneNumber == validPhone &&
                user.DateOfBirth == dateOfBirth &&
                user.Address == address &&
                user.Nationality == nationality &&
                user.PreferredLanguage == preferredLanguage)),
            Times.Once);
    }

    [Fact]
    public void LinkOAuth_WhenGoogleIsNotLinked_SavesGoogleLink()
    {
        // Arrange
        const int userId = 1;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com" });
        this.userRepository
            .Setup(repository => repository.GetLinkedProviders(userId))
            .Returns(new List<OAuthLink>());
        this.userRepository
            .Setup(repository => repository.SaveOAuthLink(userId, "Google", It.IsAny<string>(), "ada@test.com"))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.LinkOAuth(userId, "Google");

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Verify(repository => repository.SaveOAuthLink(userId, "Google", It.IsAny<string>(), "ada@test.com"), Times.Once);
    }

    [Fact]
    public void LinkOAuth_WhenProviderIsUnsupported_ReturnsValidationError()
    {
        // Act
        ErrorOr<Success> result = this.service.LinkOAuth(1, "Facebook");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("unsupported_provider");
    }

    [Fact]
    public void UnlinkOAuth_WhenGoogleIsLinked_DeletesLink()
    {
        // Arrange
        const int userId = 1;
        const int linkId = 7;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId });
        this.userRepository
            .Setup(repository => repository.GetLinkedProviders(userId))
            .Returns(new List<OAuthLink> { new OAuthLink { Id = linkId, Provider = "Google" } });
        this.userRepository
            .Setup(repository => repository.DeleteOAuthLink(linkId))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.UnlinkOAuth(userId, "Google");

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Verify(repository => repository.DeleteOAuthLink(linkId), Times.Once);
    }

    [Fact]
    public void ChangePassword_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());
        var request = new ChangePasswordRequest(99, "old", "NewPass1!");

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
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "hash" });
        var request = new ChangePasswordRequest(userId, "old", "weak");

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
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "hash" });
        this.hashService
            .Setup(service => service.Verify("wrongpassword", "hash"))
            .Returns((ErrorOr<bool>)false);
        var request = new ChangePasswordRequest(userId, "wrongpassword", "NewPass1!");

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
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "oldhash" });
        this.hashService
            .Setup(service => service.Verify("oldpassword", "oldhash"))
            .Returns((ErrorOr<bool>)true);
        this.hashService
            .Setup(service => service.GetHash(newPassword))
            .Returns((ErrorOr<string>)newHash);
        this.userRepository
            .Setup(repository => repository.UpdatePassword(userId, newHash))
            .Returns(Result.Success);
        var request = new ChangePasswordRequest(userId, "oldpassword", newPassword);

        // Act
        ErrorOr<Success> result = this.service.ChangePassword(request);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Verify(repository => repository.UpdatePassword(userId, newHash), Times.Once);
    }

    [Fact]
    public void Enable2FA_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = this.service.Enable2FA(99, TwoFactorMethod.Email);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Enable2FA_WhenUserExists_EnablesTwoFactorAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        this.userRepository
            .Setup(repository => repository.UpdateUser(It.IsAny<User>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.Enable2FA(userId, TwoFactorMethod.Email);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Verify(
            repository => repository.UpdateUser(It.Is<User>(user =>
                user.Is2FAEnabled && user.Preferred2FAMethod == "Email")),
            Times.Once);
    }

    [Fact]
    public void Disable2FA_WhenUserExists_DisablesTwoFactorAndReturnsSuccess()
    {
        // Arrange
        const int userId = 1;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", Is2FAEnabled = true });
        this.userRepository
            .Setup(repository => repository.UpdateUser(It.IsAny<User>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.Disable2FA(userId);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Verify(
            repository => repository.UpdateUser(It.Is<User>(user =>
                !user.Is2FAEnabled && user.Preferred2FAMethod == null)),
            Times.Once);
    }

    [Fact]
    public void VerifyPassword_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<bool> result = this.service.VerifyPassword(99, "anypassword");

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordMatches_ReturnsTrue()
    {
        // Arrange
        const int userId = 1;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "correcthash" });
        this.hashService
            .Setup(service => service.Verify("correctpassword", "correcthash"))
            .Returns((ErrorOr<bool>)true);

        // Act
        ErrorOr<bool> result = this.service.VerifyPassword(userId, "correctpassword");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WhenPasswordDoesNotMatch_ReturnsFalse()
    {
        // Arrange
        const int userId = 1;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada", PasswordHash = "correcthash" });
        this.hashService
            .Setup(service => service.Verify("wrongpassword", "correcthash"))
            .Returns((ErrorOr<bool>)false);

        // Act
        ErrorOr<bool> result = this.service.VerifyPassword(userId, "wrongpassword");

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public void GetNotificationPreferences_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<List<NotificationPreferenceDto>> result = this.service.GetNotificationPreferences(99);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void GetNotificationPreferences_WhenUserExists_ReturnsMappedPreferences()
    {
        // Arrange
        const int userId = 1;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        this.userRepository
            .Setup(repository => repository.GetNotificationPreferences(userId))
            .Returns(new List<NotificationPreference>
            {
                new NotificationPreference { Id = 1, UserId = userId, Category = NotificationType.Payment, EmailEnabled = true },
            });

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
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<List<SessionDto>> result = this.service.GetActiveSessions(99);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void GetActiveSessions_WhenUserExists_ReturnsMappedSessionDtos()
    {
        // Arrange
        const int userId = 1;
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        this.userRepository
            .Setup(repository => repository.GetActiveSessions(userId))
            .Returns(new List<Session>
            {
                new Session { Id = 1, UserId = userId, Token = "token1", DeviceInfo = "Chrome/Windows" },
            });

        // Act
        ErrorOr<List<SessionDto>> result = this.service.GetActiveSessions(userId);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().ContainSingle();
        result.Value[0].Id.Should().Be(1);
        result.Value[0].DeviceInfo.Should().Be("Chrome/Windows");
    }

    [Fact]
    public void RevokeSession_WhenUserDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        this.userRepository
            .Setup(repository => repository.FindById(99))
            .Returns(Error.NotFound());

        // Act
        ErrorOr<Success> result = this.service.RevokeSession(99, 1);

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
        this.userRepository
            .Setup(repository => repository.FindById(userId))
            .Returns(new User { Id = userId, Email = "ada@test.com", FullName = "Ada" });
        this.userRepository
            .Setup(repository => repository.RevokeSession(userId, sessionId))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.RevokeSession(userId, sessionId);

        // Assert
        result.IsError.Should().BeFalse();
        this.userRepository.Verify(repository => repository.RevokeSession(userId, sessionId), Times.Once);
    }
}
