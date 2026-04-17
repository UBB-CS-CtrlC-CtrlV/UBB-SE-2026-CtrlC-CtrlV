// <copyright file="UserMessages.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Centralises all user-facing message strings shown by ViewModels.
/// Keeps display text out of the call sites and in one place so it is
/// easy to review, update, or replace with a proper localisation layer later.
/// </summary>
internal static class UserMessages
{
    /// <summary>Messages shown in the dashboard view.</summary>
    internal static class Dashboard
    {
        /// <summary>Shown when the server rejects the request as unauthorised (expired session).</summary>
        internal const string SessionExpired = "Your session expired. Please sign in again.";

        /// <summary>Shown when the server cannot find dashboard data for this account.</summary>
        internal const string NotFound = "Dashboard data was not found for this account.";

        /// <summary>Shown for any other load failure.</summary>
        internal const string LoadFailed = "We couldn't load your dashboard. Please try again.";

        /// <summary>Shown when the dashboard response is missing required data.</summary>
        internal const string IncompleteResponse = "The dashboard response was incomplete.";
    }

    /// <summary>Messages shown in the two-factor authentication view.</summary>
    internal static class TwoFactor
    {
        /// <summary>Shown when the user submits a code that is not exactly 6 digits.</summary>
        internal const string InvalidCodeFormat = "Please enter a valid 6-digit code."; // To Do: Replace magic number 6 with constant

        /// <summary>Shown when the server rejects the submitted code.</summary>
        internal const string IncorrectCode = "The code you entered is incorrect.";
    }

    /// <summary>Messages shown in the registration view.</summary>
    internal static class Register
    {
        /// <summary>Shown when the email address is already associated with an account.</summary>
        internal const string EmailAlreadyExists = "This email is already registered.";

        /// <summary>Shown when the entered email address is not valid.</summary>
        internal const string InvalidEmail = "Please enter a valid email address.";

        /// <summary>Shown when the chosen password does not meet strength requirements.</summary>
        internal const string WeakPassword = "Password must be at least 8 characters with uppercase, lowercase, a digit and a special character."; // To Do: Replace magic number 8 with constant

        /// <summary>Shown when the password and confirmation password do not match.</summary>
        internal const string PasswordMismatch = "Passwords do not match.";

        /// <summary>Shown when one or more required fields are left blank.</summary>
        internal const string AllFieldsRequired = "Please fill in all fields.";
    }

    /// <summary>Messages shown in the profile view.</summary>
    internal static class Profile
    {
        /// <summary>Shown when no phone number has been set for the user.</summary>
        internal const string NoPhoneNumber = "No phone number set";
    }

    /// <summary>Messages shown for security-related operations.</summary>
    internal static class Security
    {
        /// <summary>Shown when the new password does not meet the minimum length requirement.</summary>
        internal const string MinimumLengthRequired = "Minimum 8 characters required."; // To Do: Replace magic number 8 with constant

        /// <summary>Shown when the new password and confirmation do not match.</summary>
        internal const string PasswordMismatch = "Passwords do not match.";

        /// <summary>Shown when the current password verification fails.</summary>
        internal const string IncorrectPassword = "Current password is incorrect.";

        /// <summary>Shown when an unexpected error occurs during a security operation.</summary>
        internal const string UnexpectedError = "An unexpected error occurred.";
    }

    /// <summary>Messages shown in the forgot-password view.</summary>
    internal static class ForgotPassword
    {
        /// <summary>Shown when the email field is empty on submission.</summary>
        internal const string EmailRequired = "Please enter your email address.";

        /// <summary>Shown when required fields are left empty on the reset form.</summary>
        internal const string AllFieldsRequired = "Please fill in all fields.";

        /// <summary>Shown when the chosen password does not meet the complexity requirements.</summary>
        internal const string PasswordTooWeak = "Password must be at least 8 characters with uppercase, lowercase, a digit, and a special character."; // To Do: Replace magic number 8 with constant

        /// <summary>Shown when the user tries to verify without pasting a recovery code.</summary>
        internal const string CodeRequired = "Please paste the recovery code first.";
    }
}
