// <copyright file="DiagnosticsController.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
using BankApp.Server.DataAccess;
using BankApp.Server.DataAccess.Interfaces;
using BankApp.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers;

/// <summary>
/// Temporary diagnostics controller used exclusively for development and testing purposes.
/// Provides endpoints for verifying database connectivity, user lookup, hashing, and JWT functionality.
/// This controller must be removed before any production deployment.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    /// <summary>
    /// Tests the database connection by executing a simple query against the User table.
    /// </summary>
    /// <param name="db">The application database context injected from services.</param>
    /// <returns>
    /// 200 OK with a connection confirmation and user count on success,
    /// or 500 Internal Server Error if the query fails.
    /// </returns>
    [HttpGet("db")]
    public IActionResult TestDb([FromServices] AppDbContext db)
    {
        try
        {
            var count = db.ExecuteQuery("SELECT COUNT(*) FROM [User]", Array.Empty<object>(), reader =>
            {
                reader.Read();
                return reader.GetInt32(0);
            });
            return this.Ok(new { message = "Connection works!", userCount = count });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    /// <summary>
    /// Looks up a user in the database by their email address and returns their details.
    /// </summary>
    /// <param name="userDao">The user data access object injected from services.</param>
    /// <param name="email">The email address of the user to find.</param>
    /// <returns>
    /// 200 OK with user details on success,
    /// 404 Not Found if no user with the given email exists,
    /// or 500 Internal Server Error if the lookup fails.
    /// </returns>
    [HttpGet("user/find/{email}")]
    public IActionResult FindUser([FromServices] IUserDataAccess userDao, string email)
    {
        try
        {
            var user = userDao.FindByEmail(email);
            if (user == null)
            {
                return this.NotFound(new { error = $"User with email '{email}' not found" });
            }

            return this.Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.PhoneNumber,
                user.Is2FAEnabled,
                user.IsLocked,
                user.FailedLoginAttempts,
            });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tests the hashing service by hashing the provided text and verifying the result.
    /// </summary>
    /// <param name="hash">The hashing service injected from services.</param>
    /// <param name="text">The plain text string to hash and verify.</param>
    /// <returns>
    /// 200 OK with the original text, its hash, and verification result on success,
    /// or 500 Internal Server Error if hashing fails.
    /// </returns>
    [HttpGet("hash/{text}")]
    public IActionResult TestHash([FromServices] IHashService hash, string text)
    {
        try
        {
            var hashed = hash.GetHash(text);
            var verified = hash.Verify(text, hashed);
            return this.Ok(new { original = text, hash = hashed, verified });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tests JWT token generation for a given user ID.
    /// </summary>
    /// <param name="jwt">The JWT service injected from services.</param>
    /// <param name="userId">The ID of the user to generate a token for.</param>
    /// <returns>
    /// 200 OK with the user ID and generated token on success,
    /// or 500 Internal Server Error if token generation fails.
    /// </returns>
    [HttpGet("jwt/generate/{userId}")]
    public IActionResult TestJwtGenerate([FromServices] IJwtService jwt, int userId)
    {
        try
        {
            var token = jwt.GenerateToken(userId);
            return this.Ok(new { userId, token });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Tests JWT token validation by extracting and returning the user ID from the provided token.
    /// </summary>
    /// <param name="jwt">The JWT service injected from services.</param>
    /// <param name="token">The JWT token string to validate.</param>
    /// <returns>
    /// 200 OK with the validity status and extracted user ID on success,
    /// or 500 Internal Server Error if validation fails.
    /// </returns>
    [HttpGet("jwt/validate/{token}")]
    public IActionResult TestJwtValidate([FromServices] IJwtService jwt, string token)
    {
        try
        {
            var userId = jwt.ExtractUserId(token);
            return this.Ok(new { valid = userId != null, userId });
        }
        catch (Exception ex)
        {
            return this.StatusCode(500, new { error = ex.Message });
        }
    }
}