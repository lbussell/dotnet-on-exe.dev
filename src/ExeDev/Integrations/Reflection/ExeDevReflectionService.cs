// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;

namespace LoganBussell.ExeDev.Integrations.Reflection;

/// <summary>
/// Default <see cref="IExeDevReflection"/> implementation backed by an
/// <see cref="HttpClient"/> pointed at the reflection integration. Each call
/// fetches fresh data because VM metadata can change over time.
/// </summary>
public sealed class ExeDevReflectionService(HttpClient httpClient, ILogger<ExeDevReflectionService> logger)
    : IExeDevReflection
{
    /// <summary>Base address of the exe.dev reflection integration.</summary>
    public static readonly Uri BaseAddress = new("https://reflection.int.exe.xyz/");

    /// <summary>Maximum time to wait for reflection integration responses.</summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public async Task<string?> GetOwnerEmailAsync(CancellationToken cancellationToken = default)
    {
        EmailResponse? response = await FetchAsync(
            "email",
            ExeDevReflectionJsonContext.Default.EmailResponse,
            cancellationToken
        );
        return response?.Email;
    }

    public async Task<IReadOnlyList<ExeDevIntegration>> GetIntegrationsAsync(
        CancellationToken cancellationToken = default
    )
    {
        IntegrationsResponse? response = await FetchAsync(
            "integrations",
            ExeDevReflectionJsonContext.Default.IntegrationsResponse,
            cancellationToken
        );
        return response?.Integrations ?? [];
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        TagsResponse? response = await FetchAsync(
            "tags",
            ExeDevReflectionJsonContext.Default.TagsResponse,
            cancellationToken
        );
        return response?.Tags ?? [];
    }

    public async Task<string?> GetCommentAsync(CancellationToken cancellationToken = default)
    {
        CommentResponse? response = await FetchAsync(
            "comment",
            ExeDevReflectionJsonContext.Default.CommentResponse,
            cancellationToken
        );
        return response?.Comment;
    }

    public async Task<int?> GetDefaultPortAsync(CancellationToken cancellationToken = default)
    {
        DefaultPortResponse? response = await FetchAsync(
            "default_port",
            ExeDevReflectionJsonContext.Default.DefaultPortResponse,
            cancellationToken
        );
        return response?.DefaultPort;
    }

    private async Task<T?> FetchAsync<T>(string path, JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken)
        where T : class
    {
        try
        {
            return await httpClient.GetFromJsonAsync(path, typeInfo, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            logger.LogWarning(ex, "Timed out reading '{Path}' from the exe.dev reflection integration.", path);
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning(ex, "Unable to read '{Path}' from the exe.dev reflection integration.", path);
            return null;
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Invalid JSON reading '{Path}' from the exe.dev reflection integration.", path);
            return null;
        }
        catch (NotSupportedException ex)
        {
            logger.LogWarning(ex, "Unable to read '{Path}' from the exe.dev reflection integration.", path);
            return null;
        }
    }
}