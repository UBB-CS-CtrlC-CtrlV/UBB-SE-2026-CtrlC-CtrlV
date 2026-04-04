// <copyright file="AppNavigationService.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace BankApp.Client.Master;

/// <summary>
/// Provides navigation services for the application, enabling switching between pages and managing navigation
/// history.
/// </summary>
/// <remarks>
/// Pages are resolved through the DI container so that their constructor dependencies
/// (e.g. view models) are injected automatically. Navigation is performed by setting
/// <see cref="Frame.Content"/> directly rather than calling <see cref="Frame.Navigate(Type)"/>,
/// because the latter instantiates pages via reflection and bypasses the container.
/// </remarks>
public class AppNavigationService : IAppNavigationService
{
    private readonly IServiceProvider serviceProvider;

    private Frame? frame;
    private Frame? contentFrame;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppNavigationService"/> class.
    /// </summary>
    /// <param name="serviceProvider">
    /// The DI container used to resolve page instances with their injected dependencies.
    /// </param>
    public AppNavigationService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public void SetFrame(Frame newFrame)
    {
        this.frame = newFrame;
    }

    /// <inheritdoc />
    public void SetContentFrame(Frame newFrame)
    {
        this.contentFrame = newFrame;
    }

    /// <inheritdoc />
    public void NavigateTo<TPage>()
        where TPage : class
    {
        var page = this.serviceProvider.GetRequiredService<TPage>();
        this.frame!.Content = page;
    }

    /// <inheritdoc />
    public void NavigateToContent<TPage>()
        where TPage : class
    {
        var page = this.serviceProvider.GetRequiredService<TPage>();
        this.contentFrame!.Content = page;
    }
}
