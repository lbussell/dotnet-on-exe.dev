// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace LoganBussell.ExeDev;

/// <summary>
/// Helpers for reading the exe.dev authenticated user from a claims principal.
/// </summary>
public static class ExeDevClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the authenticated exe.dev user from the principal.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the principal is not authenticated or does not contain the required exe.dev claims.
    /// </exception>
    public static ExeDevUser GetExeDevUser(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.TryGetExeDevUser(out ExeDevUser? user))
        {
            return user;
        }

        throw new InvalidOperationException("The authenticated principal does not contain the required exe.dev user claims.");
    }

    /// <summary>
    /// Attempts to get the authenticated exe.dev user from the principal.
    /// </summary>
    public static bool TryGetExeDevUser(this ClaimsPrincipal principal, [NotNullWhen(true)] out ExeDevUser? user)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.Identity?.IsAuthenticated != true)
        {
            user = null;
            return false;
        }

        string? userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        string? email = principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            user = null;
            return false;
        }

        user = new ExeDevUser(userId, email);
        return true;
    }
}