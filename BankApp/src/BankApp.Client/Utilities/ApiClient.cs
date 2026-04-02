using System.Net.Http;
using System.Net.Http.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace BankApp.Client.Utilities
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private string? _token;
        private int? _currentUserId;

        public ApiClient(string baseUrl = "http://localhost:5024")
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public void SetToken(string token)
        {
            _token = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void SetCurrentUserId(int userId)
        {
            _currentUserId = userId;
        }

        public int? GetCurrentUserId()
        {
            return _currentUserId;
        }

        public void ClearToken()
        {
            _token = null;
            _currentUserId = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public string? GetToken()
        {
            return _token;
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                // Put a breakpoint here and check ex.Message
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
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new ApiException(
                    response.StatusCode,
                    string.IsNullOrWhiteSpace(errorBody)
                        ? $"Request to '{endpoint}' failed with status {(int)response.StatusCode}."
                        : errorBody);
            }

            return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<TResponse>();

            // Try to read error response
            return await response.Content.ReadFromJsonAsync<TResponse>();
        }
    }

    public class ApiException : Exception
    {
        public ApiException(HttpStatusCode statusCode, string message)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }
}
