// <copyright file="MainWindow.xaml.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Client.Views;
using Microsoft.UI.Xaml;

namespace BankApp.Client;

/// <summary>
/// Hosts the application's root frame and initializes the first navigated view.
/// </summary>
public sealed partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        this.InitializeComponent();
        App.NavigationService.SetFrame(this.RootFrame);

        // Start on the login page
        App.NavigationService.NavigateTo<LoginView>();
    }
}
