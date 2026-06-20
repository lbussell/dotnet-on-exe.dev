// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using LoganBussell.ExeDev.Integrations.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoganBussell.ExeDev.Authorization;

/// <summary>
/// Extension methods for registering exe.dev owner authorization.
/// </summary>
public static class ExeDevAuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds an authorization policy named
    /// <see cref="ExeDevAuthorizationDefaults.OwnerPolicy"/> that is satisfied
    /// only by the VM owner. Also registers the reflection service the policy
    /// depends on.
    /// </summary>
    public static IServiceCollection AddExeDevOwnerPolicy(this IServiceCollection services)
    {
        services.AddExeDevReflection();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, OwnerAuthorizationHandler>());

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                ExeDevAuthorizationDefaults.OwnerPolicy,
                policy => policy.RequireAuthenticatedUser().AddRequirements(new OwnerRequirement())
            );

        return services;
    }
}