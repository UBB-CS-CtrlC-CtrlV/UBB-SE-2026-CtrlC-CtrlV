// <copyright file="LoopbackHttpListener.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Hosts a temporary loopback HTTP endpoint used to capture browser-based authentication callbacks.
/// </summary>
public partial class LoopbackHttpListener : IDisposable
{
    private const int DefaultTimeout = 60 * 5;
    private readonly HttpListener listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopbackHttpListener"/> class.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <param name="path">The path.</param>
    public LoopbackHttpListener(int port, string? path = null)
    {
        path = path ?? string.Empty;
        if (path.StartsWith("/"))
        {
            path = path[1..];
        }

        var primaryListenerUrl = $"http://127.0.0.1:{port}/{path}";

        this.listener = new HttpListener();
        this.listener.Prefixes.Add(primaryListenerUrl);
        if (!primaryListenerUrl.EndsWith('/'))
        {
            this.listener.Prefixes.Add(primaryListenerUrl + "/");
        }

        this.listener.Start();
    }

    /// <summary>
    /// Waits for the first callback request received by the loopback listener.
    /// </summary>
    /// <param name="timeoutInSeconds">Timeout period in seconds.</param>
    /// <returns>The callback URL received from the browser.</returns>
    public async Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));
        var contextTask = Task.Factory.FromAsync(
            this.listener.BeginGetContext,
            this.listener.EndGetContext,
            null);
        var completedTask = await Task.WhenAny(contextTask, Task.Delay(Timeout.Infinite, cancellationTokenSource.Token));

        if (completedTask != contextTask)
        {
            throw new TaskCanceledException("Browser authentication timed out.");
        }

        var context = await contextTask;
        await this.WriteCompletionResponseAsync(context.Response);
        return context.Request.Url!.ToString();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.listener.Stop();
    }

    private async Task WriteCompletionResponseAsync(HttpListenerResponse response)
    {
        const string responseString = "<html><head><style>body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;background:#f0f2f5;} .card{background:white;padding:2rem;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,0.1);text-align:center;}</style></head><body><div class='card'><h2>Authentication Complete!</h2><p>You can now safely close this browser tab and return to the Bank App.</p></div></body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}
