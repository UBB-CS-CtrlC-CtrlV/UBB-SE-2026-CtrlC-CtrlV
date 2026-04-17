// <copyright file="VerifyTokenDataTransferObject.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DataTransferObjects.Auth;

/// <summary>
/// Data transfer object used for reset token verification requests.
/// </summary>
public class VerifyTokenDataTransferObject
{
    /// <summary>
    /// Gets or sets the reset token to be verified.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}