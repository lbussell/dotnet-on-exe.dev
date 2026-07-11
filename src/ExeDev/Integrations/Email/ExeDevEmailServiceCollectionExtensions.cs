// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoganBussell.ExeDev.Integrations.Email;

/// <summary>
/// Extension methods for registering the exe.dev email service.
/// </summary>
public static class ExeDevEmailServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IExeDevEmailService"/>, which sends plain-text email through the exe.dev VM
    /// email gateway. Safe to call multiple times.
    /// </summary>
    /// <remarks>
    /// Uses a dedicated named client managed by <see cref="IHttpClientFactory"/> and does not replace
    /// other default, named, or typed clients. The email service is safe to inject into singleton
    /// hosted services because it obtains a short-lived client for each send.
    /// </remarks>
    public static IServiceCollection AddExeDevEmail(this IServiceCollection services)
    {
        services.AddHttpClient(ExeDevEmailService.HttpClientName, client =>
        {
            client.BaseAddress = ExeDevEmailService.BaseAddress;
            client.Timeout = ExeDevEmailService.Timeout;
        });
        services.TryAddSingleton<IExeDevEmailService, ExeDevEmailService>();

        return services;
    }
}