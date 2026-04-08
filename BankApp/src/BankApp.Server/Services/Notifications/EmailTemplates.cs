// <copyright file="EmailTemplates.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Server.Services.Security;

namespace BankApp.Server.Services.Notifications;

/// <summary>
/// Defines the subjects and body templates for all transactional emails sent by the application.
/// </summary>
public static class EmailTemplates
{
    /// <summary>OTP validity period in minutes, derived from <see cref="OtpService.TotpWindowSeconds"/>.</summary>
    internal const int OtpValidityMinutes = OtpService.TotpWindowSeconds / 60;

    /// <summary>Subject for the account-locked notification.</summary>
    public const string AccountLockedSubject = "BankApp - Account Locked";

    /// <summary>Body for the account-locked notification.</summary>
    public const string AccountLockedBody =
        "Hello,\n\nYour account has been temporarily locked due to multiple failed login attempts. " +
        "Please try again later or reset your password.";

    /// <summary>Subject for the new-login alert.</summary>
    public const string LoginAlertSubject = "BankApp - New Login Detected";

    /// <summary>Body for the new-login alert.</summary>
    public const string LoginAlertBody =
        "Hello,\n\nWe detected a new login to your BankApp account. " +
        "If this was you, no action is needed. " +
        "If this wasn't you, please change your password immediately.";

    /// <summary>Subject for the OTP delivery email.</summary>
    public const string OtpSubject = "Your BankApp Login Code";

    /// <summary>Subject for the password-reset email.</summary>
    public const string PasswordResetSubject = "BankApp - Password Reset Code";

    /// <summary>
    /// Returns the body for the OTP delivery email, embedding the generated <paramref name="code"/>
    /// and the validity window derived from <see cref="OtpService.TotpWindowSeconds"/>.
    /// </summary>
    /// <param name="code">The one-time password to include in the message.</param>
    /// <returns>The formatted email body.</returns>
    public static string GetOtpBody(string code) =>
        $"Hello,\n\nYour One-Time Password (OTP) is: {code}\n\n" +
        $"This code is valid for {OtpValidityMinutes} minutes. Do not share it with anyone.";

    /// <summary>
    /// Returns the body for the password-reset email, embedding the raw <paramref name="token"/>.
    /// </summary>
    /// <param name="token">The password-reset token to include in the message.</param>
    /// <returns>The formatted email body.</returns>
    public static string GetPasswordResetBody(string token) =>
        $"Hello,\n\nYou requested a password reset. " +
        $"Please copy and paste the recovery code below into the app:\n\n{token}\n\n" +
        "If you did not request this, please ignore this email.";
}
