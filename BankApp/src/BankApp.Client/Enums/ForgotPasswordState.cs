// <copyright file="ForgotPasswordState.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Client.Enums;

/// <summary>
/// Represents the UI state of the forgot-password flow.
/// </summary>
public enum ForgotPasswordState
{
    /// <summary>
    /// No forgot-password request has been initiated.
    /// </summary>
    Idle,

    /// <summary>
    /// The reset email was sent successfully.
    /// </summary>
    EmailSent,

    /// <summary>
    /// The provided reset token is valid.
    /// </summary>
    TokenValid,

    /// <summary>
    /// The provided reset token is invalid.
    /// </summary>
    TokenInvalid,

    /// <summary>
    /// The provided reset token has expired.
    /// </summary>
    TokenExpired,

    /// <summary>
    /// The provided reset token has already been used.
    /// </summary>
    TokenAlreadyUsed,

    /// <summary>
    /// The password was reset successfully.
    /// </summary>
    PasswordResetSuccess,

    /// <summary>
    /// An unexpected error occurred during the forgot-password flow.
    /// </summary>
    Error,
}
