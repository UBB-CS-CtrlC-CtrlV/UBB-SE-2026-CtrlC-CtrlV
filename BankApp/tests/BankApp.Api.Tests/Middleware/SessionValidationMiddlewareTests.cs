// <copyright file="SessionValidationMiddlewareTests.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Api.Middleware;
using BankApp.Application.Repositories.Interfaces;
using BankApp.Application.Services.Security;
using BankApp.Domain.Entities;
using ErrorOr;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BankApp.Api.Tests.Middleware;

/// <summary>
/// Unit tests for <see cref="SessionValidationMiddleware"/> verifying
/// public-versus-protected endpoint behavior and auth token validation.
/// </summary>
[Trait("Category", "Unit")]
public sealed class SessionValidationMiddlewareTests
{
    private readonly Mock<IAuthRepository> authenticationRepository = MockFactory.CreateAuthRepository();
    private readonly Mock<IJsonWebTokenService> jwtService = MockFactory.CreateJwtService();
    private readonly Mock<ILogger<SessionValidationMiddleware>> logger = new Mock<ILogger<SessionValidationMiddleware>>();

    private bool nextWasCalled;

    /// <summary>
    /// Verifies the Invoke_PublicEndpoint_CallsNextWithoutToken scenario.
    /// </summary>
    /// <param name="path">The request path that should bypass session validation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [InlineData("/auth/login")]
    [InlineData("/auth/register")]
    [InlineData("/swagger/index.html")]
    [InlineData("/test/health")]
    public async Task Invoke_PublicEndpoint_CallsNextWithoutToken(string path)
    {
        // Arrange
        SessionValidationMiddleware middleware = this.CreateMiddleware();
        HttpContext context = CreateHttpContext(path);

        // Act
        await middleware.Invoke(context, this.authenticationRepository.Object, this.jwtService.Object, this.logger.Object);

        // Assert
        this.nextWasCalled.Should().BeTrue();
        context.Response.StatusCode.Should().NotBe(401);
    }

    /// <summary>
    /// Verifies the Invoke_ProtectedEndpoint_NoToken_Returns401 scenario.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Invoke_ProtectedEndpoint_NoToken_Returns401()
    {
        // Arrange
        SessionValidationMiddleware middleware = this.CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/dashboard");

        // Act
        await middleware.Invoke(context, this.authenticationRepository.Object, this.jwtService.Object, this.logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies the Invoke_ProtectedEndpoint_InvalidToken_Returns401 scenario.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Invoke_ProtectedEndpoint_InvalidToken_Returns401()
    {
        // Arrange
        this.jwtService
            .Setup(service => service.ExtractUserId("bad-token"))
            .Returns(Error.Validation("token_invalid", "Invalid token."));

        SessionValidationMiddleware middleware = this.CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/profile", "Bearer bad-token");

        // Act
        await middleware.Invoke(context, this.authenticationRepository.Object, this.jwtService.Object, this.logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies the Invoke_ProtectedEndpoint_ValidTokenButNoSession_Returns401 scenario.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Invoke_ProtectedEndpoint_ValidTokenButNoSession_Returns401()
    {
        // Arrange
        var validUserId = 1;
        this.jwtService.Setup(service => service.ExtractUserId("good-token")).Returns(validUserId);
        this.authenticationRepository
            .Setup(repository => repository.FindSessionByToken("good-token"))
            .Returns(Error.NotFound("session_not_found", "Session not found."));

        SessionValidationMiddleware middleware = this.CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/dashboard", "Bearer good-token");

        // Act
        await middleware.Invoke(context, this.authenticationRepository.Object, this.jwtService.Object, this.logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }

    /// <summary>
    /// Verifies the Invoke_ProtectedEndpoint_ValidTokenAndSession_CallsNextAndSetsUserId scenario.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Invoke_ProtectedEndpoint_ValidTokenAndSession_CallsNextAndSetsUserId()
    {
        // Arrange
        var validUserId = 42;
        var validSessionId = 1;
        this.jwtService.Setup(service => service.ExtractUserId("good-token")).Returns(validUserId);
        this.authenticationRepository
            .Setup(repository => repository.FindSessionByToken("good-token"))
            .Returns(new Session { Id = validSessionId, UserId = validUserId, Token = "good-token" });

        SessionValidationMiddleware middleware = this.CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/profile", "Bearer good-token");

        // Act
        await middleware.Invoke(context, this.authenticationRepository.Object, this.jwtService.Object, this.logger.Object);

        // Assert
        this.nextWasCalled.Should().BeTrue();
        context.Items["UserId"].Should().Be(validUserId);
    }

    /// <summary>
    /// Verifies the Invoke_ProtectedEndpoint_MalformedAuthHeader_Returns401 scenario.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Invoke_ProtectedEndpoint_MalformedAuthHeader_Returns401()
    {
        // Arrange
        SessionValidationMiddleware middleware = this.CreateMiddleware();
        HttpContext context = CreateHttpContext("/api/dashboard", "Basic some-creds");

        // Act
        await middleware.Invoke(context, this.authenticationRepository.Object, this.jwtService.Object, this.logger.Object);

        // Assert
        context.Response.StatusCode.Should().Be(401);
        this.nextWasCalled.Should().BeFalse();
    }

    private static HttpContext CreateHttpContext(string path, string? authorizationHeader = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        if (authorizationHeader != null)
        {
            context.Request.Headers.Authorization = authorizationHeader;
        }

        return context;
    }

    private SessionValidationMiddleware CreateMiddleware()
    {
        this.nextWasCalled = false;
        return new SessionValidationMiddleware(_ =>
        {
            this.nextWasCalled = true;
            return Task.CompletedTask;
        });
    }
}
