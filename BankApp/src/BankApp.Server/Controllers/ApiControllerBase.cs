// <copyright file="ApiControllerBase.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using BankApp.Contracts.DTOs;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers;

/// <summary>
/// Base class for all API controllers, providing shared helpers for authenticated requests
/// and for mapping <see cref="ErrorOr{T}"/> results to <see cref="IActionResult"/> responses.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Extracts the authenticated user's ID from the HTTP context,
    /// set by the session validation middleware.
    /// </summary>
    /// <returns>The ID of the currently authenticated user.</returns>
    protected int GetAuthenticatedUserId() => (int)this.HttpContext.Items["UserId"] !;

    /// <summary>
    /// Maps a fire-and-forget <see cref="ErrorOr{Success}"/> result to an HTTP response.
    /// Returns 204 No Content on success or the appropriate error status on failure.
    /// </summary>
    /// <param name="result">The result to map.</param>
    /// <returns>204 No Content, or an error response.</returns>
    protected IActionResult ToActionResult(ErrorOr<Success> result)
    {
        return result.IsError ? this.MapError(result.FirstError) : this.NoContent();
    }

    /// <summary>
    /// Maps a value-bearing <see cref="ErrorOr{T}"/> result to an HTTP response using a
    /// caller-supplied success handler.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="onSuccess">
    /// A function that receives the success value and returns the <see cref="IActionResult"/> to send.
    /// </param>
    /// <returns>The result of <paramref name="onSuccess"/> on success, or an error response.</returns>
    protected IActionResult ToActionResult<T>(ErrorOr<T> result, Func<T, IActionResult> onSuccess)
    {
        return result.IsError ? this.MapError(result.FirstError) : onSuccess(result.Value);
    }

    /// <summary>
    /// Maps an <see cref="Error"/> to the appropriate HTTP status code and
    /// a standardised <see cref="ApiErrorResponse"/> body.
    /// </summary>
    /// <param name="error">The error to map.</param>
    /// <returns>An <see cref="IActionResult"/> with the matching HTTP status code.</returns>
    protected IActionResult MapError(Error error)
    {
        ApiErrorResponse body = new ApiErrorResponse
        {
            Error = error.Description,
            ErrorCode = error.Code,
        };

        return error.Type switch
        {
            ErrorType.Unauthorized => this.Unauthorized(body),
            ErrorType.Forbidden => this.StatusCode(StatusCodes.Status403Forbidden, body),
            ErrorType.Validation => this.BadRequest(body),
            ErrorType.Conflict => this.Conflict(body),
            ErrorType.NotFound => this.NotFound(body),
            _ => this.StatusCode(StatusCodes.Status500InternalServerError, body),
        };
    }
}
