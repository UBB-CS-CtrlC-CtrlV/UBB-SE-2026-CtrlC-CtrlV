// <copyright file="ApiClient.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BankApp.Client.Utilities
{
    /// <summary>
    /// Provides a thin wrapper around <see cref="HttpClient"/> for the application's API calls.
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient httpClient;
        private string? token;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClient"/> class.
        /// </summary>
        /// <param name="baseUrl">The API base URL.</param>
        public ApiClient(string baseUrl = "http://localhost:5024")
        {
            this.httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
            };
        }

        /// <summary>
        /// Gets or sets the identifier of the currently authenticated user.
        /// </summary>
        public int? CurrentUserId { get; set; }

        /// <summary>
        /// Gets the currently configured bearer token.
        /// </summary>
        /// <returns>The configured bearer token, if one exists.</returns>
        public string? Token { get; }

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
        /// <returns>The deserialized response body, if available.</returns>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var response = await this.httpClient.PostAsJsonAsync(endpoint, data);
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP ERROR: {ex.Message}");
                Console.WriteLine($"Inner: {ex.InnerException?.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sends a GET request to the provided endpoint and deserializes the response body.
        /// </summary>
        /// <typeparam name="TResponse">The response model type.</typeparam>
        /// <param name="endpoint">The relative endpoint to call.</param>
        /// <param name="cancellationToken">A token that can cancel the request.</param>
        /// <returns>The deserialized response body, if available.</returns>
        public virtual async Task<TResponse?> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default)
        {
            var response = await this.httpClient.GetAsync(endpoint, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var errorMessage = string.IsNullOrWhiteSpace(errorBody)
                ? $"Request to '{endpoint}' failed with status {(int)response.StatusCode}."
                : errorBody;

            throw new ApiException(response.StatusCode, errorMessage);
        }

        /// <summary>
        /// Sends a PUT request to the provided endpoint and deserializes the response body.
        /// </summary>
        /// <typeparam name="TRequest">The request model type.</typeparam>
        /// <typeparam name="TResponse">The response model type.</typeparam>
        /// <param name="endpoint">The relative endpoint to call.</param>
        /// <param name="data">The request body to serialize.</param>
        /// <returns>The deserialized response body, if available.</returns>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await this.httpClient.PutAsJsonAsync(endpoint, data);
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
    }
}
