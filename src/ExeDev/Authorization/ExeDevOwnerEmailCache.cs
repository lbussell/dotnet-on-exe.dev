// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using LoganBussell.ExeDev.Integrations.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace LoganBussell.ExeDev.Authorization;

/// <summary>
/// Caches the exe.dev Reflection owner email for the process lifetime.
/// </summary>
public sealed class ExeDevOwnerEmailCache(IServiceScopeFactory scopeFactory)
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private string? _ownerEmail;

    /// <summary>
    /// Gets the cached owner email, loading it from Reflection on the first successful lookup.
    /// </summary>
    public async Task<string?> GetOwnerEmailAsync(CancellationToken cancellationToken = default)
    {
        string? cachedOwnerEmail = Volatile.Read(ref _ownerEmail);
        if (!string.IsNullOrEmpty(cachedOwnerEmail))
        {
            return cachedOwnerEmail;
        }

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            cachedOwnerEmail = Volatile.Read(ref _ownerEmail);
            if (!string.IsNullOrEmpty(cachedOwnerEmail))
            {
                return cachedOwnerEmail;
            }

            using IServiceScope scope = scopeFactory.CreateScope();
            IExeDevReflection reflection = scope.ServiceProvider.GetRequiredService<IExeDevReflection>();
            string? ownerEmail = await reflection.GetOwnerEmailAsync(cancellationToken);
            if (!string.IsNullOrEmpty(ownerEmail))
            {
                Volatile.Write(ref _ownerEmail, ownerEmail);
            }

            return ownerEmail;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}