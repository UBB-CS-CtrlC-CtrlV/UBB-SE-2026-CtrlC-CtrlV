// <copyright file="IPasswordRecoveryManager.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using BankApp.Core.Enums;

namespace BankApp.Client.Utilities
{
    /// <summary>
    /// Manages the business logic for the password-recovery flow, including
    /// state transitions, token validation, and resend throttling.
    /// </summary>
    public interface IPasswordRecoveryManager
    {
        /// <summary>
        /// Gets a value indicating whether the user is allowed to request a new recovery code.
        /// Returns <see langword="false"/> within the 60-second cooldown after the last request.
        /// </summary>
        bool CanResendCode { get; }

        /// <summary>
        /// Gets the number of seconds remaining before the user is allowed to resend a recovery code.
        /// Returns 0 when resending is already permitted.
        /// </summary>
        int SecondsUntilResendAllowed { get; }

        /// <summary>
        /// Requests a password-recovery code for the given email address.
        /// Enforces throttling: if called within 60 seconds of a previous request the
        /// current state is returned unchanged without making a network call.
        /// </summary>
        /// <param name="email">The email address to send the recovery code to.</param>
        /// <returns>The new <see cref="ForgotPasswordState"/> after the operation.</returns>
        Task<ForgotPasswordState> RequestCodeAsync(string email);

        /// <summary>
        /// Validates the supplied recovery token without consuming it.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>The new <see cref="ForgotPasswordState"/> after the operation.</returns>
        Task<ForgotPasswordState> VerifyTokenAsync(string token);

        /// <summary>
        /// Resets the user's password using a previously verified token.
        /// </summary>
        /// <param name="token">The validated reset token.</param>
        /// <param name="newPassword">The new password to apply.</param>
        /// <returns>The new <see cref="ForgotPasswordState"/> after the operation.</returns>
        Task<ForgotPasswordState> ResetPasswordAsync(string token, string newPassword);

        /// <summary>
        /// Validates a plain-text password against the application's complexity rules.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns><see langword="true"/> if the password meets all requirements; otherwise <see langword="false"/>.</returns>
        bool IsPasswordValid(string password);
    }
}
