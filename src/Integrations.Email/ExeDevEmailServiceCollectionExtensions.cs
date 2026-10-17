// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

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

    /// <summary>
    /// Registers a hosted service that dispatches emails from the exe.dev VM's Maildir to a scoped
    /// <typeparamref name="THandler"/> instance.
    /// </summary>
    /// <remarks>
    /// Pending deliveries are handled at least once. A delivery is moved from <c>new</c> to
    /// <c>cur</c> only after the handler returns successfully; otherwise it remains pending for
    /// retry.
    /// </remarks>
    /// <typeparam name="THandler">The scoped email handler implementation.</typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">Configures Maildir handling.</param>
    public static IServiceCollection AddExeDevEmailHandler<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] THandler
    >(
        this IServiceCollection services,
        Action<ExeDevEmailHandlerOptions>? configure = null
    )
        where THandler : class, IExeDevEmailHandler
    {
        var options = services
            .AddOptions<ExeDevEmailHandlerOptions>()
            .Validate(
                static options => !string.IsNullOrWhiteSpace(options.MaildirPath),
                $"{nameof(ExeDevEmailHandlerOptions.MaildirPath)} must not be empty."
            )
            .Validate(
                static options => options.PollingInterval > TimeSpan.Zero,
                $"{nameof(ExeDevEmailHandlerOptions.PollingInterval)} must be greater than zero."
            )
            .ValidateOnStart();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        services.AddLogging();
        services.TryAddScoped<IExeDevEmailHandler, THandler>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<
                IHostedService,
                ExeDevEmailHandlerBackgroundService
            >()
        );

        return services;
    }
}