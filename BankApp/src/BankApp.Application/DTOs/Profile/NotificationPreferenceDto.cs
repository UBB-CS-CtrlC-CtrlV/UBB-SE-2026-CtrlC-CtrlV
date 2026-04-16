// <copyright file="NotificationPreferenceDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Domain.Enums;

namespace BankApp.Application.DTOs.Profile;

/// <summary>
/// Data transfer object representing a user's notification preference for a specific category.
/// </summary>
public class NotificationPreferenceDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the preference.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user this preference belongs to.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the notification category this preference applies to.
    /// </summary>
    public NotificationType Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether push notifications are enabled.
    /// </summary>
    public bool PushEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether email notifications are enabled.
    /// </summary>
    public bool EmailEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether SMS notifications are enabled.
    /// </summary>
    public bool SmsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum amount threshold that triggers a notification.
    /// </summary>
    public decimal? MinAmountThreshold { get; set; }
}
