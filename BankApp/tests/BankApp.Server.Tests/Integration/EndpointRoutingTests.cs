// <copyright file="EndpointRoutingTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BankApp.Contracts.Entities;
using BankApp.Server.Tests.Integration.Infrastructure;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace BankApp.Server.Tests.Integration;

/// <summary>
/// Integration tests that verify route contracts, middleware auth enforcement,
/// and the public-versus-protected distinction by sending real HTTP requests
/// through the full ASP.NET Core pipeline.
/// </summary>
public class EndpointRoutingTests : IClassFixture<BankAppWebFactory>
{
    private const string ValidToken = "valid-test-token";

    private readonly HttpClient client;
    private readonly BankAppWebFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointRoutingTests"/> class.
    /// </summary>
    /// <param name="factory">The shared web application factory.</param>
    public EndpointRoutingTests(BankAppWebFactory factory)
    {
        this.factory = factory;
        this.client = factory.CreateClient();

        // By default, configure the substitutes so that a valid token is accepted.
        factory.JwtServiceSub.ExtractUserId(ValidToken)
            .Returns(1);

        factory.AuthRepositorySub.FindSessionByToken(ValidToken)
            .Returns(new Session { UserId = 1, Token = ValidToken });
    }

    // ------------------------------------------------------------------
    // Public auth endpoints – should be reachable without a token
    // ------------------------------------------------------------------

    /// <summary>
    /// Auth endpoints are public; the middleware must let requests through without a bearer token.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="path">Relative request path.</param>
    [Theory]
    [InlineData("POST", "/api/auth/login")]
    [InlineData("POST", "/api/auth/register")]
    [InlineData("POST", "/api/auth/verify-otp")]
    [InlineData("POST", "/api/auth/forgot-password")]
    [InlineData("POST", "/api/auth/reset-password")]
    [InlineData("POST", "/api/auth/logout")]
    [InlineData("POST", "/api/auth/resend-otp")]
    [InlineData("POST", "/api/auth/oauth-login")]
    [InlineData("POST", "/api/auth/verify-reset-token")]
    public async Task PublicAuthEndpoint_WithoutToken_DoesNotReturn401(string method, string path)
    {
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = JsonContent.Create(new { }),
        };

        HttpResponseMessage response = await this.client.SendAsync(request);

        // The endpoint is reachable (middleware did not reject). We accept any
        // status other than 401, because the empty body may cause a 400 or 500.
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    // ------------------------------------------------------------------
    // Protected endpoints – must return 401 without a token
    // ------------------------------------------------------------------

    /// <summary>
    /// Protected endpoints must be rejected by the session middleware when no token is provided.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="path">Relative request path.</param>
    [Theory]
    [InlineData("GET", "/api/dashboard")]
    [InlineData("GET", "/api/profile")]
    [InlineData("PUT", "/api/profile")]
    [InlineData("PUT", "/api/profile/password")]
    [InlineData("GET", "/api/profile/oauth-links")]
    [InlineData("GET", "/api/profile/notifications/preferences")]
    [InlineData("PUT", "/api/profile/notifications/preferences")]
    [InlineData("POST", "/api/profile/verify-password")]
    [InlineData("PUT", "/api/profile/2fa/enable")]
    [InlineData("PUT", "/api/profile/2fa/disable")]
    [InlineData("GET", "/api/profile/sessions")]
    [InlineData("DELETE", "/api/profile/sessions/1")]
    public async Task ProtectedEndpoint_WithoutToken_Returns401(string method, string path)
    {
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), path);

        HttpResponseMessage response = await this.client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ------------------------------------------------------------------
    // Protected endpoints – must pass through middleware with a valid token
    // ------------------------------------------------------------------

    /// <summary>
    /// When a valid bearer token is provided the middleware should pass the request
    /// through to the controller, so the response must not be 401.
    /// </summary>
    /// <param name="method">HTTP method.</param>
    /// <param name="path">Relative request path.</param>
    [Theory]
    [InlineData("GET", "/api/dashboard")]
    [InlineData("GET", "/api/profile")]
    [InlineData("PUT", "/api/profile")]
    [InlineData("PUT", "/api/profile/password")]
    [InlineData("GET", "/api/profile/oauth-links")]
    [InlineData("GET", "/api/profile/notifications/preferences")]
    [InlineData("PUT", "/api/profile/notifications/preferences")]
    [InlineData("POST", "/api/profile/verify-password")]
    [InlineData("PUT", "/api/profile/2fa/enable")]
    [InlineData("PUT", "/api/profile/2fa/disable")]
    [InlineData("GET", "/api/profile/sessions")]
    [InlineData("DELETE", "/api/profile/sessions/1")]
    public async Task ProtectedEndpoint_WithValidToken_DoesNotReturn401(string method, string path)
    {
        HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        // Provide minimal JSON body for endpoints that expect one.
        if (method is "POST" or "PUT")
        {
            request.Content = JsonContent.Create(new { });
        }

        HttpResponseMessage response = await this.client.SendAsync(request);

        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    // ------------------------------------------------------------------
    // Middleware rejects invalid / expired tokens
    // ------------------------------------------------------------------

    /// <summary>
    /// A malformed or expired token should be rejected with 401.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_Returns401()
    {
        this.factory.JwtServiceSub.ExtractUserId("bad-token")
            .Returns(Error.Unauthorized("Token.Invalid", "Token is invalid."));

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "bad-token");

        HttpResponseMessage response = await this.client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// A valid JWT whose session no longer exists in the database should be rejected.
    /// </summary>
    [Fact]
    public async Task ProtectedEndpoint_WithExpiredSession_Returns401()
    {
        const string orphanToken = "orphan-token";

        this.factory.JwtServiceSub.ExtractUserId(orphanToken)
            .Returns(1);

        this.factory.AuthRepositorySub.FindSessionByToken(orphanToken)
            .Returns(Error.NotFound("Session.NotFound", "Session not found."));

        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/dashboard");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", orphanToken);

        HttpResponseMessage response = await this.client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ------------------------------------------------------------------
    // Unknown routes
    // ------------------------------------------------------------------

    /// <summary>
    /// A request to a non-existent route without a token is intercepted by the
    /// session middleware before routing, so it returns 401 rather than 404.
    /// This verifies the middleware applies to all non-public paths.
    /// </summary>
    [Fact]
    public async Task NonExistentRoute_WithoutToken_Returns401()
    {
        HttpResponseMessage response = await this.client.GetAsync("/api/does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// A request to a non-existent route with a valid token passes the middleware
    /// but finds no matching endpoint, resulting in 404.
    /// </summary>
    [Fact]
    public async Task NonExistentRoute_WithValidToken_Returns404()
    {
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/api/does-not-exist");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ValidToken);

        HttpResponseMessage response = await this.client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
