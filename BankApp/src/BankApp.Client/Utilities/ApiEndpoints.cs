// <copyright file="ApiEndpoints.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Client.Utilities;

/// <summary>
/// Defines the relative API endpoint paths used by the client.
/// </summary>
public static class ApiEndpoints
{
    /// <summary>POST api/auth/login — credential login.</summary>
    public const string Login = "api/auth/login";

    /// <summary>POST api/auth/oauth-login — OAuth provider login or registration.</summary>
    public const string OAuthLogin = "api/auth/oauth-login";

    /// <summary>POST api/auth/oauth-register — OAuth provider registration.</summary>
    public const string OAuthRegister = "api/auth/oauth-register";

    /// <summary>POST api/auth/register — new account registration.</summary>
    public const string Register = "api/auth/register";

    /// <summary>POST api/auth/forgot-password — request a password-reset code.</summary>
    public const string ForgotPassword = "api/auth/forgot-password";

    /// <summary>POST api/auth/verify-reset-token — validate a password-reset token.</summary>
    public const string VerifyResetToken = "api/auth/verify-reset-token";

    /// <summary>POST api/auth/reset-password — apply a new password via reset token.</summary>
    public const string ResetPassword = "api/auth/reset-password";

    /// <summary>POST api/auth/verify-otp — submit a one-time password for 2FA.</summary>
    public const string VerifyOtp = "api/auth/verify-otp";

    /// <summary>POST api/auth/resend-otp — request a new OTP code.</summary>
    public const string ResendOtp = "api/auth/resend-otp";

    /// <summary>GET api/dashboard/ — load the authenticated user's dashboard data.</summary>
    public const string Dashboard = "api/dashboard/";

    /// <summary>GET/PUT api/profile/ — load or update the user's profile.</summary>
    public const string Profile = "api/profile/";

    /// <summary>POST api/profile/verify-password — verify the user's current password.</summary>
    public const string VerifyPassword = "api/profile/verify-password";

    /// <summary>PUT api/profile/password — change the user's password.</summary>
    public const string ChangePassword = "api/profile/password";

    /// <summary>PUT api/profile/2fa/enable — enable two-factor authentication.</summary>
    public const string Enable2Fa = "api/profile/2fa/enable";

    /// <summary>PUT api/profile/2fa/disable — disable two-factor authentication.</summary>
    public const string Disable2Fa = "api/profile/2fa/disable";

    /// <summary>GET api/profile/oauth-links — list linked OAuth providers.</summary>
    public const string OAuthLinks = "api/profile/oauth-links";

    /// <summary>POST api/profile/oauth/link — link a new OAuth provider.</summary>
    public const string LinkOAuth = "api/profile/oauth/link";

    /// <summary>GET/PUT api/profile/notifications/preferences — load or update notification preferences.</summary>
    public const string NotificationPreferences = "api/profile/notifications/preferences";

    /// <summary>GET api/profile/sessions — list active sessions. DELETE api/profile/sessions/{id} — revoke a session.</summary>
    public const string Sessions = "api/profile/sessions";
}
