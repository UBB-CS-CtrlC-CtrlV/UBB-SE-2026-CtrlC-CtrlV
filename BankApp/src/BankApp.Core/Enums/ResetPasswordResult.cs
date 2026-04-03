// <copyright file="ResetPasswordResult.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Core.Enums
{
    /// <summary>
    /// Represents the result of attempting to reset a password with a reset token.
    /// </summary>
    public enum ResetPasswordResult
    {
        Success,
        InvalidToken,
        ExpiredToken,
        TokenAlreadyUsed,
    }
}
