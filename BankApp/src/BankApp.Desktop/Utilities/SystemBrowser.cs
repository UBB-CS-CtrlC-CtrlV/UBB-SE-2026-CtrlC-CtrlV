// <copyright file="SystemBrowser.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityModel.OidcClient.Browser;

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Implementation used to invoke the system default browser.
/// </summary>
public class SystemBrowser : IBrowser
{
    private readonly string? path;
    private readonly int port;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemBrowser"/> class.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <param name="path">The path.</param>
    public SystemBrowser(int? port = null, string? path = null)
    {
        this.path = path;
        this.port = port ?? this.GetRandomUnusedPort();
    }

    /// <inheritdoc/>
    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        using var listener = new LoopbackHttpListener(this.port, this.path);

        OpenBrowser(options.StartUrl);

        try
        {
            var result = await listener.WaitForCallbackAsync();

            if (string.IsNullOrWhiteSpace(result))
            {
                return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = "Empty response." };
            }

            return new BrowserResult { Response = result, ResultType = BrowserResultType.Success };
        }
        catch (TaskCanceledException exception)
        {
            return new BrowserResult { ResultType = BrowserResultType.Timeout, Error = exception.Message };
        }
        catch (Exception exception)
        {
            return new BrowserResult { ResultType = BrowserResultType.UnknownError, Error = exception.Message };
        }
    }

    private static void OpenBrowser(string browserAddress)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = browserAddress,
                UseShellExecute = true,
            });
    }

    private int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, default(int));
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
