// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;

namespace LoganBussell.ExeDev.Integrations.Reflection;

/// <summary>
/// Default <see cref="IExeDevReflection"/> implementation backed by an
/// <see cref="HttpClient"/> pointed at the reflection integration.
/// </summary>
public sealed class ExeDevReflectionService(HttpClient httpClient) : IExeDevReflection
{
    /// <summary>Base address of the exe.dev reflection integration.</summary>
    public static readonly Uri BaseAddress = new("https://reflection.int.exe.xyz/");

    /// <summary>Maximum time to wait for reflection integration responses.</summary>
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public async Task<string?> GetOwnerEmailAsync(CancellationToken cancellationToken = default)
    {
        EmailResponse? response = await httpClient.GetFromJsonAsync(
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
        IntegrationsResponse? response = await httpClient.GetFromJsonAsync(
            "integrations",
            ExeDevReflectionJsonContext.Default.IntegrationsResponse,
            cancellationToken
        );
        return response?.Integrations ?? [];
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        TagsResponse? response = await httpClient.GetFromJsonAsync(
            "tags",
            ExeDevReflectionJsonContext.Default.TagsResponse,
            cancellationToken
        );
        return response?.Tags ?? [];
    }

    public async Task<string?> GetCommentAsync(CancellationToken cancellationToken = default)
    {
        CommentResponse? response = await httpClient.GetFromJsonAsync(
            "comment",
            ExeDevReflectionJsonContext.Default.CommentResponse,
            cancellationToken
        );
        return response?.Comment;
    }

    public async Task<int?> GetDefaultPortAsync(CancellationToken cancellationToken = default)
    {
        DefaultPortResponse? response = await httpClient.GetFromJsonAsync(
            "default_port",
            ExeDevReflectionJsonContext.Default.DefaultPortResponse,
            cancellationToken
        );
        return response?.DefaultPort;
    }
}