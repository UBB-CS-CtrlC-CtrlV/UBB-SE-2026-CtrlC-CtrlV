// <copyright file="TwoFactorState.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Desktop.Enums;

/// <summary>
/// Represents the possible states of the two-factor authentication flow.
/// </summary>
public enum TwoFactorState
{
    /// <summary>
    /// Awaiting the user to enter their one-time password.
    /// </summary>
    Idle,

    /// <summary>
    /// The OTP is being verified against the server.
    /// </summary>
    Verifying,

    /// <summary>
    /// Verification succeeded. The user should be navigated to the main application.
    /// </summary>
    Success,

    /// <summary>
    /// The submitted OTP did not match the expected value.
    /// </summary>
    InvalidOTP,     // To Do: Change to OTP

    /// <summary>
    /// The OTP was valid but has passed its expiry window.
    /// </summary>
    Expired,

    /// <summary>
    /// Too many failed verification attempts have been made. Further attempts are blocked.
    /// </summary>
    MaxAttemptsReached,
}
