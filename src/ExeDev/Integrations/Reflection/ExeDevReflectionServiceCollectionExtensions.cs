// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddExeDevReflection(this IServiceCollection services)
    {
        services.AddHttpClient<IExeDevReflection, ExeDevReflectionService>(client =>
        {
            client.BaseAddress = ExeDevReflectionService.BaseAddress;
        });

        return services;
    }
}
