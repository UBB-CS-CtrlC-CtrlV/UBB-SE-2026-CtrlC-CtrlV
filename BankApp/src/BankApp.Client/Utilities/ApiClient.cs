// <copyright file="ApiClient.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankApp.Client.Utilities;

/// <summary>
/// Provides a thin wrapper around <see cref="HttpClient"/> for the application's API calls.
/// </summary>
public class ApiClient
{
    private readonly HttpClient httpClient;
    private readonly ILogger<ApiClient> logger;
    private readonly Error? configurationError;
    private string? token;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiClient"/> class.
    /// </summary>
    /// <param name="configuration">
    /// The application configuration. Reads <c>ApiBaseUrl</c> to set the HTTP base address.
    /// If the key is absent the client starts in a degraded state — callers must check
    /// <see cref="EnsureConfigured"/> before issuing requests.
    /// </param>
    /// <param name="logger">Logger for HTTP errors and configuration warnings.</param>
    public ApiClient(IConfiguration configuration, ILogger<ApiClient> logger)
    {
        this.logger = logger;

        var baseUrl = configuration["ApiBaseUrl"];
        if (baseUrl is null)
        {
            this.configurationError = Error.Failure(code: "ApiClient.MissingBaseUrl",
                description: "ApiBaseUrl is missing from configuration.");

            this.logger.LogCritical(
                "ApiBaseUrl is missing from configuration. The client cannot connect to the server.");

            // Dummy client — requests must not be issued when configurationError is set.
            this.httpClient = new HttpClient();
        }
        else
        {
            this.httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }
    }

    /// <summary>
    /// Returns <see cref="Success"/> when the client is correctly configured,
    /// or a <see cref="Error.Failure"/> describing the missing configuration otherwise.
    /// Callers should check this before issuing any requests.
    /// </summary>
    public ErrorOr<Success> EnsureConfigured() =>
        this.configurationError is null ? Result.Success : this.configurationError.Value;

    /// <summary>
    /// Gets or sets the identifier of the currently authenticated user.
    /// </summary>
    public int? CurrentUserId { get; set; }

    /// <summary>
    /// Gets the currently configured bearer token.
    /// </summary>
    public string? Token => this.token;

    /// <summary>
    /// Gets the identifier of the currently authenticated user.
    /// </summary>
    /// <returns>The authenticated user identifier, if one exists.</returns>
    public int? GetCurrentUserId()
    {
        return this.CurrentUserId;
    }

    /// <summary>
    /// Sets the identifier of the currently authenticated user.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    public void SetCurrentUserId(int userId)
    {
        this.CurrentUserId = userId;
    }

    /// <summary>
    /// Sets the bearer token used for authenticated requests.
    /// </summary>
    /// <param name="tokenStr">The token value.</param>
    public void SetToken(string tokenStr)
    {
        this.token = tokenStr;
        this.httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenStr);
    }

    /// <summary>
    /// Clears the stored authentication state from the client.
    /// </summary>
    public void ClearToken()
    {
        this.token = null;
        this.CurrentUserId = null;
        this.httpClient.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Sends a POST request to the provided endpoint and deserializes the response body.
    /// </summary>
    /// <typeparam name="TRequest">The request model type.</typeparam>
    /// <typeparam name="TResponse">The response model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="data">The request body to serialize.</param>
    /// <returns>The deserialized response body, or an <see cref="Error"/> if the request fails.</returns>
    public async Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await this.httpClient.PostAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var parsed = await response.Content.ReadFromJsonAsync<TResponse>();
                    if (parsed is not null)
                    {
                        return parsed;
                    }
                }

                return await MapErrorAsync(response, endpoint, CancellationToken.None);
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>();
            if (result is null)
            {
                return Error.Failure(description: $"POST {endpoint} returned an empty response.");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            return Error.Failure(description: $"POST {endpoint} failed: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected(description: $"POST {endpoint} was cancelled.");
        }
    }

    /// <summary>
    /// Sends a GET request to the provided endpoint and deserializes the response body.
    /// </summary>
    /// <typeparam name="TResponse">The response model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="cancellationToken">
    /// Used to cancel the in-flight HTTP request. Defaults to <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>The deserialized response body, or an <see cref="Error"/> if the request fails.</returns>
    public virtual async Task<ErrorOr<TResponse>> GetAsync<TResponse>(
        string endpoint,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await this.httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(response, endpoint, cancellationToken);
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            if (result is null)
            {
                return Error.Failure(description: $"GET {endpoint} returned an empty response.");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            return Error.Failure(description: $"GET {endpoint} failed: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Failure(description: $"GET {endpoint} was cancelled.");
        }
    }

    private static async Task<Error> MapErrorAsync(HttpResponseMessage response,
        string endpoint,
        CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = string.IsNullOrWhiteSpace(body)
            ? $"Request to '{endpoint}' failed with status {(int)response.StatusCode}."
            : body;

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => Error.NotFound(description: message),
            HttpStatusCode.Unauthorized => Error.Unauthorized(description: message),
            HttpStatusCode.Forbidden => Error.Forbidden(description: message),
            HttpStatusCode.Conflict => Error.Conflict(description: message),
            HttpStatusCode.UnprocessableEntity => Error.Validation(description: message),
            HttpStatusCode.TooManyRequests => Error.Failure(description: message),
            >= HttpStatusCode.InternalServerError => Error.Unexpected(description: message),
            _ => Error.Failure(description: message),
        };
    }

    /// <summary>
    /// Sends a PUT request to the provided endpoint and deserializes the response body.
    /// </summary>
    /// <typeparam name="TRequest">The request model type.</typeparam>
    /// <typeparam name="TResponse">The response model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="data">The request body to serialize.</param>
    /// <returns>The deserialized response body, or an <see cref="Error"/> if the request fails.</returns>
    public async Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        try
        {
            var response = await this.httpClient.PutAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
            {
                return await MapErrorAsync(response, endpoint, CancellationToken.None);
            }

            var result = await response.Content.ReadFromJsonAsync<TResponse>();
            if (result is null)
            {
                return Error.Failure(description: $"PUT {endpoint} returned an empty response.");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            return Error.Failure(description: $"PUT {endpoint} failed: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return Error.Unexpected(description: $"PUT {endpoint} was cancelled.");
        }
    }
}