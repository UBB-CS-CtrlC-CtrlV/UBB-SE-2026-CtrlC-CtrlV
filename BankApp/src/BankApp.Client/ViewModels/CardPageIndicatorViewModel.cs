// <copyright file="CardPageIndicatorViewModel.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Client.ViewModels;

/// <summary>
/// Represents the display state of a single card-navigation dot in the dashboard carousel.
/// </summary>
public class CardPageIndicatorViewModel
{
    /// <summary>
    /// Gets a value indicating whether this dot corresponds to the currently displayed card.
    /// </summary>
    public bool IsActive { get; init; }
}
