// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LoganBussell.ExeDev.Integrations.Notify;

/// <summary>
/// Extension methods for registering the exe.dev notify service.
/// </summary>
public static class ExeDevNotifyServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IExeDevNotifyService"/>, which sends push notifications through the
    /// exe.dev notify integration. Safe to call multiple times.
    /// </summary>
    /// <remarks>
    /// Uses a dedicated named client managed by <see cref="IHttpClientFactory"/> and does not replace
    /// other default, named, or typed clients. The notify service is safe to inject into singleton
    /// hosted services because it obtains a short-lived client for each send.
    /// </remarks>
    public static IServiceCollection AddExeDevNotify(this IServiceCollection services)
    {
        services.AddHttpClient(ExeDevNotifyService.HttpClientName, client =>
        {
            client.BaseAddress = ExeDevNotifyService.BaseAddress;
            client.Timeout = ExeDevNotifyService.Timeout;
        });
        services.TryAddSingleton<IExeDevNotifyService, ExeDevNotifyService>();

        return services;
    }
}
