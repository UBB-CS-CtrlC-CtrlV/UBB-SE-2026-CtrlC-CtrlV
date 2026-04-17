// <copyright file="LoginSuccessResponse.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DTOs.Auth;

/// <summary>
/// The JSON response body returned by a successful login or OTP-verification endpoint.
/// When <see cref="Requires2FA"/> is <see langword="true"/>, <see cref="Token"/> is
/// <see langword="null"/> and the client must complete the two-factor flow before a
/// token is issued.
/// </summary>
public sealed class LoginSuccessResponse
{
    /// <summary>
    /// Gets or sets the identifier of the authenticated user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the signed JWT for subsequent authenticated requests,
    /// or <see langword="null"/> when <see cref="Requires2FA"/> is <see langword="true"/>.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user must complete a two-factor
    /// authentication step before a token is issued.
    /// </summary>
    public bool Requires2FA { get; set; }
}
