// <copyright file="ResetTokenValidationResult.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Core.Enums
{
    /// <summary>
    /// Represents the validation state of a password reset token.
    /// </summary>
    public enum ResetTokenValidationResult
    {
        Valid,
        Invalid,
        Expired,
        AlreadyUsed,
    }
}
