// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace LoganBussell.ExeDev;

/// <summary>
/// Extension methods for registering the common exe.dev ASP.NET Core integration.
/// </summary>
public static class ExeDevServiceCollectionExtensions
{
    /// <summary>
    /// Adds exe.dev proxy-header authentication and the VM owner authorization policy.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configureAuthentication">Configures exe.dev authentication behavior.</param>
    /// <returns>
    /// The <see cref="AuthenticationBuilder"/> so additional schemes can be chained.
    /// </returns>
    public static AuthenticationBuilder AddExeDev(
        this IServiceCollection services,
        Action<ExeDevAuthenticationOptions>? configureAuthentication = null
    )
    {
        AuthenticationBuilder builder = services.AddExeDevAuthentication(configureAuthentication);
        services.AddExeDevOwnerPolicy();

        return builder;
    }
}
