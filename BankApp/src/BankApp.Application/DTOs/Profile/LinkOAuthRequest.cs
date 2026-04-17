// <copyright file="LinkOAuthRequest.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DTOs.Profile;

/// <summary>
/// Represents a request to link an OAuth provider to the current account.
/// </summary>
public class LinkOAuthRequest
{
    /// <summary>
    /// Gets or sets the OAuth provider to link.
    /// </summary>
    public string Provider { get; set; } = string.Empty;
}
