// <copyright file="LoopbackHttpListener.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Utilities;

/// <summary>
/// TODO: improve docs
/// Loopback Http Listener.
/// </summary>
public class LoopbackHttpListener : IDisposable
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
            path = path.Substring(1);
        }

        var url1 = $"http://127.0.0.1:{port}/{path}";

        this.listener = new HttpListener();
        this.listener.Prefixes.Add(url1);
        if (!url1.EndsWith('/'))
        {
            this.listener.Prefixes.Add(url1 + "/");
        }

        this.listener.Start();
    }

    /// <summary>
    /// Wait for callback.
    /// </summary>
    /// <param name="timeoutInSeconds">Timeout period in seconds.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public async Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
    {
        var source = new TaskCompletionSource<string>();

        // TODO: get rid of the lambda
        this.listener.BeginGetContext(
            async void (result) =>
        {
            try
            {
                var context = this.listener.EndGetContext(result);
                var request = context.Request;
                var response = context.Response;

                const string responseString = "<html><head><style>body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;background:#f0f2f5;} .card{background:white;padding:2rem;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,0.1);text-align:center;}</style></head><body><div class='card'><h2>Authentication Complete!</h2><p>You can now safely close this browser tab and return to the Bank App.</p></div></body></html>";
                var buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;

                var responseOutput = response.OutputStream;
                await responseOutput.WriteAsync(buffer, 0, buffer.Length);
                responseOutput.Close();

                source.SetResult(request.Url!.ToString());
            }
            catch (Exception ex)
            {
                source.SetException(ex);
            }
        },
            this.listener);

        await Task.WhenAny(source.Task, Task.Delay(timeoutInSeconds * 1000));

        if (!source.Task.IsCompleted)
        {
            throw new TaskCanceledException("Browser authentication timed out.");
        }

        return await source.Task;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.listener.Stop();
    }
}
