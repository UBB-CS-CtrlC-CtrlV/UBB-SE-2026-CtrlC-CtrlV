// <copyright file="ForgotPasswordState.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Core.Enums
{
    /// <summary>
    /// Represents the UI state of the forgot-password flow.
    /// </summary>
    public enum ForgotPasswordState
    {
        Idle,
        EmailSent,
        TokenValid,
        TokenInvalid,
        TokenExpired,
        TokenAlreadyUsed,
        PasswordResetSuccess,
        Error,
    }
}
