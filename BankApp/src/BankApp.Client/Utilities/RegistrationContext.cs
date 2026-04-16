// <copyright file="RegistrationContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Client.Utilities;

/// <inheritdoc />
public class RegistrationContext : IRegistrationContext
{
    /// <inheritdoc />
    public bool JustRegistered { get; set; }
}
