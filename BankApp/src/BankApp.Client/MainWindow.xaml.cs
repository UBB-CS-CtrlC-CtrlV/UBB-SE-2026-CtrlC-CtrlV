// <copyright file="MainWindow.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Client.Master;
using BankApp.Client.Views;

namespace BankApp.Client;

/// <summary>
/// Hosts the application's root frame and initializes the first navigated view.
/// </summary>
public sealed partial class MainWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class,
    /// wires the navigation controls and sets the current page to the <see cref="LoginView"/>.
    /// </summary>
    /// <param name="navigationService">
    /// Provides page navigation controls to the <see cref="RootFrame"/>.
    /// </param>
    public MainWindow(IAppNavigationService navigationService)
    {
        this.InitializeComponent();

        navigationService.SetFrame(this.RootFrame);

        navigationService.NavigateTo<LoginView>();
    }
}
