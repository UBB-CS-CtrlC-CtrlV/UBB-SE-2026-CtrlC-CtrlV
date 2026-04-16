// <copyright file="VerifyTokenDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DTOs.Auth;

/// <summary>
/// Data transfer object used for reset token verification requests.
/// </summary>
public class VerifyTokenDto
{
    /// <summary>
    /// Gets or sets the reset token to be verified.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}