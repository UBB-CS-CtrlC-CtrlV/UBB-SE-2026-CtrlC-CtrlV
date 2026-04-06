// <copyright file="RegisterState.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Client.Enums;

/// <summary>
/// Represents the possible states of the registration flow.
/// </summary>
public enum RegisterState
{
    /// <summary>
    /// No registration attempt is in progress. The form is idle and awaiting user input.
    /// </summary>
    Idle,

    /// <summary>
    /// A registration request is in progress.
    /// </summary>
    Loading,

    /// <summary>
    /// Registration completed successfully. The user should be prompted to log in.
    /// </summary>
    Success,

    /// <summary>
    /// The provided email address is already associated with an existing account.
    /// </summary>
    EmailAlreadyExists,

    /// <summary>
    /// The provided email address does not have a valid format.
    /// </summary>
    InvalidEmail,

    /// <summary>
    /// The provided password does not meet the minimum strength requirements.
    /// </summary>
    WeakPassword,

    /// <summary>
    /// The password and confirmation password fields do not match.
    /// </summary>
    PasswordMismatch,

    /// <summary>
    /// An unexpected error occurred during registration.
    /// </summary>
    Error,

    /// <summary>
    /// Registration via OAuth succeeded and the user was automatically signed in.
    /// </summary>
    AutoLoggedIn,
}
