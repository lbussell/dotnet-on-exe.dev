// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace LoganBussell.ExeDev.Authentication;

/// <summary>
/// Extension methods for registering exe.dev proxy-header authentication.
/// </summary>
public static class ExeDevAuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Adds authentication that derives the current user from the identity
    /// headers injected by exe.dev's HTTP proxy.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="authenticationScheme">
    /// The scheme name to register. Defaults to
    /// <see cref="ExeDevAuthenticationDefaults.AuthenticationScheme"/>.
    /// </param>
    /// <returns>
    /// The <see cref="AuthenticationBuilder"/> so additional schemes can be chained.
    /// </returns>
    public static AuthenticationBuilder AddExeDevAuthentication(
        this IServiceCollection services,
        string authenticationScheme = ExeDevAuthenticationDefaults.AuthenticationScheme
    )
    {
        return services.AddAuthentication(authenticationScheme).AddExeDevAuthentication(authenticationScheme);
    }

    /// <summary>
    /// Adds the exe.dev proxy-header authentication scheme to an existing
    /// <see cref="AuthenticationBuilder"/>.
    /// </summary>
    public static AuthenticationBuilder AddExeDevAuthentication(
        this AuthenticationBuilder builder,
        string authenticationScheme = ExeDevAuthenticationDefaults.AuthenticationScheme
    )
    {
        return builder.AddScheme<AuthenticationSchemeOptions, ExeDevAuthenticationHandler>(
            authenticationScheme,
            configureOptions: null
        );
    }
}
