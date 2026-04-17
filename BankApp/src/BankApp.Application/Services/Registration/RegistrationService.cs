// <copyright file="RegistrationService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DataTransferObjects.Auth;
using BankApp.Domain.Entities;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Security;
using BankApp.Application.Utilities;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace BankApp.Application.Services.Registration;

/// <summary>
/// Provides user registration operations, including standard and OAuth registration.
/// </summary>
public class RegistrationService : IRegistrationService
{
    private readonly IAuthRepository authRepository;
    private readonly IHashService hashService;
    private readonly ILogger<RegistrationService> logger;

    private const string DefaultLanguage = "en";
    private const string TemporaryPasswordSuffix = "A1a!";

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationService"/> class.
    /// </summary>
    /// <param name="authRepository">The authentication repository.</param>
    /// <param name="hashService">The password hashing service.</param>
    /// <param name="logger">The logger.</param>
    public RegistrationService(IAuthRepository authRepository, IHashService hashService,
        ILogger<RegistrationService> logger)
    {
        this.authRepository = authRepository;
        this.hashService = hashService;
        this.logger = logger;
    }

    /// <inheritdoc />
    public ErrorOr<Success> Register(RegisterRequest request)
    {
        Error? validationError = ValidateRegistration(request);
        if (validationError is not null)
        {
            return validationError.Value;
        }

        ErrorOr<User> existingUserResult = authRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            logger.LogInformation("Registration rejected: email already registered.");
            return Error.Conflict(code: "email_registered", description: "Email is already registered.");
        }

        if (existingUserResult.FirstError.Type != ErrorType.NotFound)
        {
            logger.LogError("Database error while checking existing user: {Error}", existingUserResult.FirstError.Description);
            return Error.Failure(code: "database_error", description: "A service error occurred. Please try again later.");
        }

        ErrorOr<User> newUserResult = CreateUserFromRequest(request);
        if (newUserResult.IsError)
        {
            return newUserResult.FirstError;
        }

        ErrorOr<Success> createResult = authRepository.CreateUser(newUserResult.Value);
        if (createResult.IsError)
        {
            logger.LogError("User creation failed during registration: {Error}", createResult.FirstError.Description);
            return Error.Failure(code: "user_creation_failed", description: "Failed to create account.");
        }

        logger.LogInformation("User registered successfully.");
        return Result.Success;
    }

    /// <inheritdoc />
    public ErrorOr<Success> OAuthRegister(OAuthRegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return Error.Validation(code: "invalid_email", description: "Invalid email format.");
        }

        ErrorOr<OAuthLink> existingLinkResult = authRepository.FindOAuthLink(request.Provider, request.ProviderToken);
        if (!existingLinkResult.IsError)
        {
            return Error.Conflict(code: "oauth_already_registered", description: "This OAuth account is already registered. Please login.");
        }

        if (existingLinkResult.FirstError.Type != ErrorType.NotFound)
        {
            logger.LogError("Database error while checking OAuth link: {Error}", existingLinkResult.FirstError.Description);
            return Error.Failure(code: "database_error", description: "A service error occurred. Please try again later.");
        }

        int targetUserId;
        ErrorOr<User> existingUserResult = authRepository.FindUserByEmail(request.Email);
        if (!existingUserResult.IsError)
        {
            targetUserId = existingUserResult.Value.Id;
        }
        else if (existingUserResult.FirstError.Type != ErrorType.NotFound)
        {
            logger.LogError("Database error while checking existing user during OAuth register: {Error}", existingUserResult.FirstError.Description);
            return Error.Failure(code: "database_error", description: "A service error occurred. Please try again later.");
        }
        else
        {
            string generatedTemporaryPassword = Guid.NewGuid().ToString() + TemporaryPasswordSuffix;
            ErrorOr<string> hashResult = hashService.GetHash(generatedTemporaryPassword);
            if (hashResult.IsError)
            {
                logger.LogError("Hash generation failed during OAuth register for provider {Provider}.", request.Provider);
                return hashResult.FirstError;
            }

            User newUser = new User
            {
                Email = request.Email,
                PasswordHash = hashResult.Value,
                FullName = request.FullName,
                PreferredLanguage = DefaultLanguage,
                Is2FAEnabled = false,
                IsLocked = false,
                FailedLoginAttempts = default,
            };

            if (authRepository.CreateUser(newUser).IsError)
            {
                return Error.Failure(code: "user_creation_failed", description: "Failed to create user account.");
            }

            ErrorOr<User> savedUserResult = authRepository.FindUserByEmail(request.Email);
            if (savedUserResult.IsError)
            {
                return Error.Failure(code: "user_retrieval_failed", description: "Error retrieving created user.");
            }

            targetUserId = savedUserResult.Value.Id;
        }

        OAuthLink newLink = new OAuthLink
        {
            UserId = targetUserId,
            Provider = request.Provider,
            ProviderUserId = request.ProviderToken,
            ProviderEmail = request.Email,
        };

        if (authRepository.CreateOAuthLink(newLink).IsError)
        {
            return Error.Failure(code: "oauth_link_failed", description: "Failed to link OAuth account to user.");
        }

        return Result.Success;
    }

    private static Error? ValidateRegistration(RegisterRequest request)
    {
        if (!ValidationUtilities.IsValidEmail(request.Email))
        {
            return Error.Validation(code: "invalid_email", description: "Invalid email format.");
        }

        if (!ValidationUtilities.IsStrongPassword(request.Password))
        {
            return Error.Validation(code: "weak_password", description: "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character.");
        }

        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return Error.Validation(code: "full_name_required", description: "Full name is required.");
        }

        return null;
    }

    private ErrorOr<User> CreateUserFromRequest(RegisterRequest request)
    {
        ErrorOr<string> hashResult = hashService.GetHash(request.Password);
        if (hashResult.IsError)
        {
            logger.LogError("Hash generation failed during registration.");
            return hashResult.FirstError;
        }

        return new User
        {
            Email = request.Email,
            PasswordHash = hashResult.Value,
            FullName = request.FullName,
            PreferredLanguage = DefaultLanguage,
            Is2FAEnabled = false,
            IsLocked = false,
            FailedLoginAttempts = default,
        };
    }
}
