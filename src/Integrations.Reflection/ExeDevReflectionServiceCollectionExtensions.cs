// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoganBussell.ExeDev.Integrations.Reflection;

/// <summary>
/// Extension methods for registering the exe.dev reflection service.
/// </summary>
public static class ExeDevReflectionServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IExeDevReflection"/>, which reads VM metadata from
    /// the exe.dev Reflection integration. Safe to call multiple times.
    /// </summary>
    /// <remarks>
    /// Uses a dedicated named client managed by <see cref="IHttpClientFactory"/> and does not replace
    /// other default, named, or typed clients. The reflection service is safe to inject into singleton
    /// hosted services because it obtains a short-lived client for each request.
    /// </remarks>
    public static IServiceCollection AddExeDevReflection(this IServiceCollection services)
    {
        services.AddHttpClient(ExeDevReflectionService.HttpClientName, client =>
        {
            client.BaseAddress = ExeDevReflectionService.BaseAddress;
            client.Timeout = ExeDevReflectionService.Timeout;
        });
        services.TryAddSingleton<IExeDevReflection, ExeDevReflectionService>();

        return services;
    }
}
