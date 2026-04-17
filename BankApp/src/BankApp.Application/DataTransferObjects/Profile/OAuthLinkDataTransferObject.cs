// <copyright file="OAuthLinkDataTransferObject.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DataTransferObjects.Profile;

/// <summary>
/// Data transfer object representing a linked OAuth provider for a user.
/// </summary>
public class OAuthLinkDataTransferObject
{
    /// <summary>
    /// Gets or sets the unique identifier of the OAuth link.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the OAuth provider name (e.g. Google, Facebook).
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address associated with the OAuth provider account.
    /// </summary>
    public string? ProviderEmail { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the OAuth link was created.
    /// </summary>
    public DateTime LinkedAt { get; set; }
}
