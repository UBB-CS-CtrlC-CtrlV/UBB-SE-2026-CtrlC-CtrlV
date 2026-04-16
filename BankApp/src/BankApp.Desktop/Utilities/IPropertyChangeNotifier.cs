// <copyright file="IPropertyChangeNotifier.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Defines a contract for notifying subscribers when a property value changes.
/// </summary>
internal interface IPropertyChangeNotifier
{
    /// <summary>
    /// Raises a property-changed notification for the given property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    void OnPropertyChanged(string propertyName);
}
