// <copyright file="TransactionDataTransferObject.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;
using BankApp.Domain.Enums;

namespace BankApp.Application.DataTransferObjects.Dashboard;

/// <summary>
/// Data transfer object representing a recent transaction on the dashboard.
/// </summary>
public class TransactionDataTransferObject
{
    /// <summary>
    /// Gets or sets the unique identifier for the transaction.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the direction of the transaction (In or Out).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionDirection Direction { get; set; }

    /// <summary>
    /// Gets or sets the amount of the transaction.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the currency code for the transaction.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the transaction.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the merchant name for card transactions.
    /// </summary>
    public string? MerchantName { get; set; }

    /// <summary>
    /// Gets or sets the name of the counterparty in the transaction.
    /// </summary>
    public string? CounterpartyName { get; set; }

    /// <summary>
    /// Gets or sets the status of the transaction.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the transaction was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
