// <copyright file="ApiException.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>

using System;
using System.Net;

namespace BankApp.Client.Utilities;

/// <summary>
/// Represents an HTTP API failure with the associated response status code.
/// </summary>
public sealed class ApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiException"/> class.
    /// </summary>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="message">The error message describing the failure.</param>
    public ApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        this.StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code returned by the API.
    /// </summary>
    public HttpStatusCode StatusCode { get; }
}
