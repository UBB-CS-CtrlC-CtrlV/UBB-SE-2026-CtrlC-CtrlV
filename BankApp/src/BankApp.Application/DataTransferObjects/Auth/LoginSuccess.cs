// <copyright file="LoginSuccess.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

namespace BankApp.Application.DataTransferObjects.Auth;

/// <summary>
/// Represents a successful login outcome. Pattern-match on the concrete type to
/// distinguish a completed login from a pending two-factor authentication step.
/// </summary>
public abstract class LoginSuccess
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginSuccess"/> class.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    protected LoginSuccess(int userId)
    {
        this.UserId = userId;
    }

    /// <summary>
    /// Gets the identifier of the authenticated user.
    /// </summary>
    public int UserId { get; }
}

/// <summary>
/// Represents a completed login where a JWT has been issued and the session is active.
/// </summary>
public sealed class FullLogin : LoginSuccess
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FullLogin"/> class.
    /// </summary>
    /// <param name="userId">The identifier of the authenticated user.</param>
    /// <param name="token">The signed JWT for subsequent authenticated requests.</param>
    public FullLogin(int userId, string token)
        : base(userId)
    {
        this.Token = token;
    }

    /// <summary>
    /// Gets the signed JWT for subsequent authenticated requests.
    /// </summary>
    public string Token { get; }
}

/// <summary>
/// Represents a partial login where two-factor authentication must be completed
/// before a token is issued.
/// </summary>
public sealed class RequiresTwoFactor : LoginSuccess
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiresTwoFactor"/> class.
    /// </summary>
    /// <param name="userId">The identifier of the partially-authenticated user.</param>
    public RequiresTwoFactor(int userId)
        : base(userId)
    {
    }
}
