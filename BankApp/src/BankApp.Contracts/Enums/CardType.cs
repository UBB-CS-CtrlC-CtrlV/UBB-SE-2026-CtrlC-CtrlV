// <copyright file="CardType.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Contracts.Enums;

/// <summary>
/// Represents the type of a payment card.
/// </summary>
public enum CardType
{
    /// <summary>
    /// A debit card linked directly to a bank account.
    /// </summary>
    Debit,

    /// <summary>
    /// A credit card backed by a line of credit.
    /// </summary>
    Credit,

    /// <summary>
    /// A prepaid card loaded with a fixed amount.
    /// </summary>
    Prepaid,
}
