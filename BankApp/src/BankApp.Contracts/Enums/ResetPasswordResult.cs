// <copyright file="ResetPasswordResult.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Contracts.Enums;

/// <summary>
/// Represents the result of attempting to reset a password with a reset token.
/// </summary>
public enum ResetPasswordResult
{
    /// <summary>
    /// The password was reset successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The provided token is invalid.
    /// </summary>
    InvalidToken,

    /// <summary>
    /// The provided token has expired.
    /// </summary>
    ExpiredToken,

    /// <summary>
    /// The provided token has already been used.
    /// </summary>
    TokenAlreadyUsed,
}