// <copyright file="IRegistrationService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Application.DTOs.Auth;
using ErrorOr;

namespace BankApp.Application.Services.Registration;

/// <summary>
/// Defines operations for user registration, including standard and OAuth registration.
/// </summary>
public interface IRegistrationService
{
    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a validation error with code <c>invalid_email</c> if the email format is invalid,
    /// a validation error with code <c>weak_password</c> if the password does not meet strength requirements,
    /// a validation error with code <c>full_name_required</c> if the full name is empty,
    /// a conflict error with code <c>email_registered</c> if the email is already in use,
    /// or a failure error if user creation fails.
    /// </returns>
    ErrorOr<Success> Register(RegisterRequest request);

    /// <summary>
    /// Registers a new user through an OAuth provider.
    /// </summary>
    /// <param name="request">The OAuth registration details.</param>
    /// <returns>
    /// <see cref="Result.Success"/> on success,
    /// a validation error if the email is invalid,
    /// a conflict error if the OAuth account is already registered,
    /// or a failure error if user or link creation fails.
    /// </returns>
    ErrorOr<Success> OAuthRegister(OAuthRegisterRequest request);
}
