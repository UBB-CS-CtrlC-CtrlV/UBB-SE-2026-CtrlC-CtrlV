// Copyright (c) BankApp. All rights reserved.
// Licensed under the MIT license.

using BankApp.Contracts.Entities;
using BankApp.Server.Middleware;
using BankApp.Server.Repositories.Interfaces;
using BankApp.Server.Services.Security;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BankApp.Server.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="SessionValidationMiddleware"/> verifying
/// public-versus-protected endpoint behavior and auth token validation.
/// </summary>
[Trait("Category", "Unit")]
public sealed class SessionValidationMiddlewareTests
{
    private readonly IAuthRepository authRepo = SubstituteFactory.CreateAuthRepository();
    private readonly IJwtService jwtService = SubstituteFactory.CreateJwtService();
    private readonly ILogger<SessionValidationMiddleware> logger = Substitute.For<ILogger<SessionValidationMiddleware>>();

    private bool nextWasCalled;

    private SessionValidationMiddleware CreateMiddleware()
    {
        this.nextWasCalled = false;
        return new SessionValidationMiddleware(_ =>
        {
            this.nextWasCalled = true;
            return Task.CompletedTask;
        });
    }

    private HttpContext CreateHttpContext(string path, string? authorizationHeader = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        if (authorizationHeader != null)
        {
            context.Request.Headers.Authorization = authorizationHeader;
        }

        return context;
    }

    [Theory]
    [InlineData("/auth/login")]
    [InlineData("/auth/register")]
    [InlineData("/swagger/index.html")]
    [InlineData("/test/health")]
    public async Task Invoke_PublicEndpoint_CallsNextWithoutToken(string path)
    {
        // Arrange
        var middleware = this.CreateMiddleware();
        var context = this.CreateHttpContext(path);

        // Act
        await middleware.Invoke(context, this.authRepo, this.jwtService, this.logger);

        // Assert
        this.nextWasCalled.Should().BeTrue();
        context.Response.StatusCode.Should().NotBe(401);
    }

    [Fact]
    public async Task Invoke_ProtectedEndpoint_NoToken_Returns401()
    {
        // Arrange
        var middleware = this.CreateMiddleware();
        var context = this.CreateHttpContext("/api/dashboard");

        // Act
        await middleware.Invoke(context, this.authRepo, this.jwtService, this.logger);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_ProtectedEndpoint_InvalidToken_Returns401()
    {
        // Arrange
        this.jwtService.ExtractUserId("bad-token")
            .Returns(Error.Validation("token_invalid", "Invalid token."));

        var middleware = this.CreateMiddleware();
        var context = this.CreateHttpContext("/api/profile", "Bearer bad-token");

        // Act
        await middleware.Invoke(context, this.authRepo, this.jwtService, this.logger);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_ProtectedEndpoint_ValidTokenButNoSession_Returns401()
    {
        // Arrange
        this.jwtService.ExtractUserId("good-token").Returns(1);
        this.authRepo.FindSessionByToken("good-token")
            .Returns(Error.NotFound("session_not_found", "Session not found."));

        var middleware = this.CreateMiddleware();
        var context = this.CreateHttpContext("/api/dashboard", "Bearer good-token");

        // Act
        await middleware.Invoke(context, this.authRepo, this.jwtService, this.logger);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Invoke_ProtectedEndpoint_ValidTokenAndSession_CallsNextAndSetsUserId()
    {
        // Arrange
        this.jwtService.ExtractUserId("good-token").Returns(42);
        this.authRepo.FindSessionByToken("good-token")
            .Returns(new Session { Id = 1, UserId = 42, Token = "good-token" });

        var middleware = this.CreateMiddleware();
        var context = this.CreateHttpContext("/api/profile", "Bearer good-token");

        // Act
        await middleware.Invoke(context, this.authRepo, this.jwtService, this.logger);

        // Assert
        this.nextWasCalled.Should().BeTrue();
        context.Items["UserId"].Should().Be(42);
    }

    [Fact]
    public async Task Invoke_ProtectedEndpoint_MalformedAuthHeader_Returns401()
    {
        // Arrange
        var middleware = this.CreateMiddleware();
        var context = this.CreateHttpContext("/api/dashboard", "Basic some-creds");

        // Act
        await middleware.Invoke(context, this.authRepo, this.jwtService, this.logger);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }
}
