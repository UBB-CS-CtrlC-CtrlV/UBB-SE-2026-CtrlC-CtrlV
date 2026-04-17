// <copyright file="CardDto.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using BankApp.Domain.Enums;

namespace BankApp.Application.DTOs.Dashboard;

/// <summary>
/// Data transfer object representing a payment card on the dashboard.
/// </summary>
public class CardDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the card.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the masked card number.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name of the account associated with this card.
    /// </summary>
    public string? AccountName { get; set; }

    /// <summary>
    /// Gets or sets the current balance of the account associated with this card.
    /// </summary>
    public decimal? AccountBalance { get; set; }

    /// <summary>
    /// Gets or sets the name of the cardholder.
    /// </summary>
    public string CardholderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the card.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardType CardType { get; set; }

    /// <summary>
    /// Gets or sets the brand of the card (e.g. Visa, Mastercard).
    /// </summary>
    public string? CardBrand { get; set; }

    /// <summary>
    /// Gets or sets the expiry date of the card.
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the status of the card.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CardStatus Status { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether contactless payments are enabled.
    /// </summary>
    public bool IsContactlessEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether online payments are enabled.
    /// </summary>
    public bool IsOnlineEnabled { get; set; }
}
