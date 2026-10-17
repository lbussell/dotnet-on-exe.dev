// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;

namespace LoganBussell.ExeDev;

/// <summary>
/// Endpoint convenience extensions for exe.dev owner authorization.
/// </summary>
public static class ExeDevOwnerEndpointConventionBuilderExtensions
{
    /// <summary>
    /// Requires that the endpoint only be accessible to the VM owner, applying
    /// the <see cref="ExeDevAuthorizationDefaults.OwnerPolicy"/> policy.
    /// </summary>
    public static TBuilder RequireExeDevOwner<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(ExeDevAuthorizationDefaults.OwnerPolicy);
    }
}
