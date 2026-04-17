// <copyright file="RegistrationServiceTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Registration;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using ErrorOr;
using Microsoft.Extensions.Logging.Abstractions;

namespace BankApp.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="RegistrationService"/>.
/// </summary>
public class RegistrationServiceTests
{
    private readonly Mock<IAuthRepository> authRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IHashService> hashService = MockFactory.CreateHashService();
    private readonly RegistrationService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationServiceTests"/> class.
    /// </summary>
    public RegistrationServiceTests()
    {
        this.service = new RegistrationService(
            this.authRepository.Object,
            this.hashService.Object,
            NullLogger<RegistrationService>.Instance);
    }

    /// <summary>
    /// Verifies the Register_WhenExistingUserLookupFails_ReturnsDatabaseError scenario.
    /// </summary>
    [Fact]
    public void Register_WhenExistingUserLookupFails_ReturnsDatabaseError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@test.com",
            Password = "StrongPass1!",
            FullName = "New User",
        };
        this.authRepository
            .Setup(repository => repository.FindUserByEmail(request.Email))
            .Returns(Error.Failure("db_failed", "Database failed."));

        // Act
        ErrorOr<Success> result = this.service.Register(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("database_error");
    }

    /// <summary>
    /// Verifies the Register_WhenValid_CreatesUserWithDefaults scenario.
    /// </summary>
    [Fact]
    public void Register_WhenValid_CreatesUserWithDefaults()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@test.com",
            Password = "StrongPass1!",
            FullName = "New User",
        };
        this.authRepository
            .Setup(repository => repository.FindUserByEmail(request.Email))
            .Returns(Error.NotFound());
        this.hashService
            .Setup(service => service.GetHash(request.Password))
            .Returns("hashed-password");
        this.authRepository
            .Setup(repository => repository.CreateUser(It.IsAny<User>()))
            .Returns(Result.Success);

        // Act
        ErrorOr<Success> result = this.service.Register(request);

        // Assert
        result.IsError.Should().BeFalse();
        this.authRepository.Verify(
            repository => repository.CreateUser(
                It.Is<User>(user =>
                    user.Email == request.Email &&
                    user.FullName == request.FullName &&
                    user.PasswordHash == "hashed-password" &&
                    user.PreferredLanguage == "en" &&
                    !user.Is2FactorAuthenticationEnabled &&
                    !user.IsLocked &&
                    user.FailedLoginAttempts == 0)),
            Times.Once);
    }

    /// <summary>
    /// Verifies the OAuthRegister_WhenLinkLookupFails_ReturnsDatabaseError scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenLinkLookupFails_ReturnsDatabaseError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
        {
            Email = "new@test.com",
            Provider = "Google",
            ProviderToken = "provider-token",
            FullName = "New User",
        };
        this.authRepository
            .Setup(repository => repository.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.Failure("db_failed", "Database failed."));

        // Act
        ErrorOr<Success> result = this.service.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("database_error");
    }

    /// <summary>
    /// Verifies the OAuthRegister_WhenExistingUserLookupFails_ReturnsDatabaseError scenario.
    /// </summary>
    [Fact]
    public void OAuthRegister_WhenExistingUserLookupFails_ReturnsDatabaseError()
    {
        // Arrange
        var request = new OAuthRegisterRequest
        {
            Email = "new@test.com",
            Provider = "Google",
            ProviderToken = "provider-token",
            FullName = "New User",
        };
        this.authRepository
            .Setup(repository => repository.FindOAuthLink(request.Provider, request.ProviderToken))
            .Returns(Error.NotFound());
        this.authRepository
            .Setup(repository => repository.FindUserByEmail(request.Email))
            .Returns(Error.Failure("db_failed", "Database failed."));

        // Act
        ErrorOr<Success> result = this.service.OAuthRegister(request);

        // Assert
        result.IsError.Should().BeTrue();
        result.FirstError.Code.Should().Be("database_error");
    }
}
