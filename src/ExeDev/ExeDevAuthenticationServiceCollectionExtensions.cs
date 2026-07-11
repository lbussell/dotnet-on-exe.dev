// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using LoganBussell.ExeDev.Authentication;

namespace LoganBussell.ExeDev;

/// <summary>
/// Extension methods for registering exe.dev proxy-header authentication.
/// </summary>
/// <remarks>
/// The scheme trusts identity headers injected by exe.dev's HTTP proxy. Applications using this
/// scheme must be reachable for protected endpoints only through that trusted proxy.
/// </remarks>
public static class ExeDevAuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Adds authentication that derives the current user from the identity
    /// headers injected by exe.dev's HTTP proxy.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configureOptions">Configures exe.dev authentication behavior.</param>
    /// <returns>
    /// The <see cref="AuthenticationBuilder"/> so additional schemes can be chained.
    /// </returns>
    public static AuthenticationBuilder AddExeDevAuthentication(
        this IServiceCollection services,
        Action<ExeDevAuthenticationOptions>? configureOptions = null
    )
    {
        return services
            .AddAuthentication(ExeDevAuthenticationDefaults.AuthenticationScheme)
            .AddExeDevAuthentication(configureOptions);
    }

    /// <summary>
    /// Adds authentication that derives the current user from the identity
    /// headers injected by exe.dev's HTTP proxy.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="authenticationScheme">
    /// The scheme name to register.
    /// </param>
    /// <param name="configureOptions">Configures exe.dev authentication behavior.</param>
    /// <returns>
    /// The <see cref="AuthenticationBuilder"/> so additional schemes can be chained.
    /// </returns>
    public static AuthenticationBuilder AddExeDevAuthentication(
        this IServiceCollection services,
        string authenticationScheme,
        Action<ExeDevAuthenticationOptions>? configureOptions = null
    )
    {
        return services.AddAuthentication(authenticationScheme).AddExeDevAuthentication(authenticationScheme, configureOptions);
    }

    /// <summary>
    /// Adds the exe.dev proxy-header authentication scheme to an existing
    /// <see cref="AuthenticationBuilder"/>.
    /// </summary>
    public static AuthenticationBuilder AddExeDevAuthentication(
        this AuthenticationBuilder builder,
        Action<ExeDevAuthenticationOptions>? configureOptions = null
    )
    {
        return builder.AddExeDevAuthentication(
            ExeDevAuthenticationDefaults.AuthenticationScheme,
            configureOptions
        );
    }

    /// <summary>
    /// Adds the exe.dev proxy-header authentication scheme to an existing
    /// <see cref="AuthenticationBuilder"/>.
    /// </summary>
    public static AuthenticationBuilder AddExeDevAuthentication(
        this AuthenticationBuilder builder,
        string authenticationScheme,
        Action<ExeDevAuthenticationOptions>? configureOptions = null
    )
    {
        return builder.AddScheme<ExeDevAuthenticationOptions, ExeDevAuthenticationHandler>(
            authenticationScheme,
            configureOptions ?? ConfigureDefaults
        );
    }

    private static void ConfigureDefaults(ExeDevAuthenticationOptions options) { }
}