// <copyright file="IApiClient.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using ErrorOr;

namespace BankApp.Desktop.Utilities;

/// <summary>
/// Defines the client-side boundary for authenticated API communication.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Gets or sets the identifier of the currently authenticated user.
    /// </summary>
    int? CurrentUserId { get; set; }

    /// <summary>
    /// Gets the currently configured bearer token.
    /// </summary>
    string? Token { get; }

    /// <summary>
    /// Returns <see cref="Success"/> when the client is correctly configured.
    /// </summary>
    /// <returns>A success result when configured; otherwise, a configuration error.</returns>
    ErrorOr<Success> EnsureConfigured();

    /// <summary>
    /// Gets the identifier of the currently authenticated user.
    /// </summary>
    /// <returns>The authenticated user identifier, if one exists.</returns>
    int? GetCurrentUserId();

    /// <summary>
    /// Sets the identifier of the currently authenticated user.
    /// </summary>
    /// <param name="userId">The authenticated user identifier.</param>
    void SetCurrentUserId(int userId);

    /// <summary>
    /// Sets the bearer token used for authenticated requests.
    /// </summary>
    /// <param name="tokenStr">The token value.</param>
    void SetToken(string tokenStr);

    /// <summary>
    /// Clears the stored authentication state from the client.
    /// </summary>
    void ClearToken();

    /// <summary>
    /// Sends a POST request and deserializes the response body into <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The request model type.</typeparam>
    /// <typeparam name="TResponse">The response model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="data">The request body to serialize.</param>
    /// <returns>The deserialized response body, or an <see cref="Error"/> if the request fails.</returns>
    Task<ErrorOr<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);

    /// <summary>
    /// Sends a POST request and returns <see cref="Success"/> when the server responds with a 2xx status.
    /// </summary>
    /// <typeparam name="TRequest">The request model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="data">The request body to serialize.</param>
    /// <returns><see cref="Result.Success"/> on a 2xx response, or an <see cref="Error"/> otherwise.</returns>
    Task<ErrorOr<Success>> PostAsync<TRequest>(string endpoint, TRequest data);

    /// <summary>
    /// Sends a GET request to the provided endpoint and deserializes the response body.
    /// </summary>
    /// <typeparam name="TResponse">The response model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="cancellationToken">Used to cancel the in-flight HTTP request.</param>
    /// <returns>The deserialized response body, or an <see cref="Error"/> if the request fails.</returns>
    Task<ErrorOr<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a PUT request and deserializes the response body into <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The request model type.</typeparam>
    /// <typeparam name="TResponse">The response model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="data">The request body to serialize.</param>
    /// <returns>The deserialized response body, or an <see cref="Error"/> if the request fails.</returns>
    Task<ErrorOr<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);

    /// <summary>
    /// Sends a PUT request and returns <see cref="Success"/> when the server responds with a 2xx status.
    /// </summary>
    /// <typeparam name="TRequest">The request model type.</typeparam>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <param name="data">The request body to serialize.</param>
    /// <returns><see cref="Result.Success"/> on a 2xx response, or an <see cref="Error"/> otherwise.</returns>
    Task<ErrorOr<Success>> PutAsync<TRequest>(string endpoint, TRequest data);

    /// <summary>
    /// Sends a DELETE request and returns <see cref="Success"/> when the server responds with a 2xx status.
    /// </summary>
    /// <param name="endpoint">The relative endpoint to call.</param>
    /// <returns><see cref="Result.Success"/> on a 2xx response, or an <see cref="Error"/> otherwise.</returns>
    Task<ErrorOr<Success>> DeleteAsync(string endpoint);
}
