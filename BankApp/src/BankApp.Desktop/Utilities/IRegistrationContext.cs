// <copyright file="IRegistrationContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Carries transient state across the register → login navigation boundary.
/// </summary>
public interface IRegistrationContext
{
    /// <summary>
    /// Gets or sets a value indicating whether the user just completed registration
    /// and the login page should display a confirmation message.
    /// </summary>
    bool JustRegistered { get; set; }
}
