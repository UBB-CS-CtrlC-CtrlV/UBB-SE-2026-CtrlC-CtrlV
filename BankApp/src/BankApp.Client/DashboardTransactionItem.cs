// <copyright file="DashboardTransactionItem.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BankApp.Client.Utilities;
using BankApp.Core.DTOs.Dashboard;
using BankApp.Core.Entities;
using BankApp.Core.Enums;

namespace BankApp.Client.ViewModels;

/// <summary>
/// Represents a formatted transaction row for dashboard display.
/// </summary>
public class DashboardTransactionItem
{
    /// <summary>
    /// Gets or sets the merchant or fallback display name.
    /// </summary>
    public string MerchantDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted amount string.
    /// </summary>
    public string AmountDisplay { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;
}