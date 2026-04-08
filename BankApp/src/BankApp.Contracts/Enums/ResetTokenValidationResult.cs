// <copyright file="ResetTokenValidationResult.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Contracts.Enums;

/// <summary>
/// Represents the validation state of a password reset token.
/// </summary>
public enum ResetTokenValidationResult
{
    /// <summary>
    /// The token is valid and can be used.
    /// </summary>
    Valid,

    /// <summary>
    /// The token is invalid or does not exist.
    /// </summary>
    Invalid,

    /// <summary>
    /// The token has expired.
    /// </summary>
    Expired,

    /// <summary>
    /// The token has already been used.
    /// </summary>
    AlreadyUsed,
}