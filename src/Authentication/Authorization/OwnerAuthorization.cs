// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using LoganBussell.ExeDev;
using LoganBussell.ExeDev.Integrations.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace LoganBussell.ExeDev.Authorization;

/// <summary>
/// Authorization requirement satisfied only by the VM owner, as reported by the
/// exe.dev Reflection integration.
/// </summary>
public sealed class OwnerRequirement : IAuthorizationRequirement;

/// <summary>
/// Grants <see cref="OwnerRequirement"/> when the authenticated user's email
/// matches the VM owner's email. Authentication establishes <em>who</em> the
/// user is; this handler decides whether that identity is the owner.
/// </summary>
public sealed class OwnerAuthorizationHandler(IExeDevReflection reflection) : AuthorizationHandler<OwnerRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerRequirement requirement
    )
    {
        if (!context.User.TryGetExeDevUser(out ExeDevUser? user))
        {
            return;
        }

        CancellationToken cancellationToken = context.Resource is HttpContext httpContext
            ? httpContext.RequestAborted
            : CancellationToken.None;

        string? ownerEmail = await reflection.GetOwnerEmailAsync(cancellationToken);
        if (string.Equals(user.Email, ownerEmail, StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }
    }
}