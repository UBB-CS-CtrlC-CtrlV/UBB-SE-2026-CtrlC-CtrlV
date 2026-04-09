using Microsoft.AspNetCore.Mvc;

namespace BankApp.Server.Controllers;

/// <summary>
/// Base class for all API controllers, providing shared helpers for authenticated requests.
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
}
