// <copyright file="SessionDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DTOs.Profile;

/// <summary>
/// Represents the safe session details exposed to profile clients.
/// </summary>
public class SessionDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the session.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the device information for the session.
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Gets or sets the browser used for the session.
    /// </summary>
    public string? Browser { get; set; }

    /// <summary>
    /// Gets or sets the IP address from which the session was created.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the last activity in this session.
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the session expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the session was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
