// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;

namespace LoganBussell.ExeDev.Integrations.Reflection;

internal sealed class ExeDevReflectionService(IHttpClientFactory httpClientFactory) : IExeDevReflection
{
    internal const string HttpClientName = "LoganBussell.ExeDev.Integrations.Reflection";
    internal static readonly Uri BaseAddress = new("https://reflection.int.exe.xyz/");
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

    public async Task<string?> GetOwnerEmailAsync(CancellationToken cancellationToken = default)
    {
        EmailResponse? response = await GetAsync(
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
        IntegrationsResponse? response = await GetAsync(
            "integrations",
            ExeDevReflectionJsonContext.Default.IntegrationsResponse,
            cancellationToken
        );
        return response?.Integrations ?? [];
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken = default)
    {
        TagsResponse? response = await GetAsync(
            "tags",
            ExeDevReflectionJsonContext.Default.TagsResponse,
            cancellationToken
        );
        return response?.Tags ?? [];
    }

    public async Task<string?> GetCommentAsync(CancellationToken cancellationToken = default)
    {
        CommentResponse? response = await GetAsync(
            "comment",
            ExeDevReflectionJsonContext.Default.CommentResponse,
            cancellationToken
        );
        return response?.Comment;
    }

    public async Task<int?> GetDefaultPortAsync(CancellationToken cancellationToken = default)
    {
        DefaultPortResponse? response = await GetAsync(
            "default_port",
            ExeDevReflectionJsonContext.Default.DefaultPortResponse,
            cancellationToken
        );
        return response?.DefaultPort;
    }

    private async Task<T?> GetAsync<T>(
        string path,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken
    )
    {
        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);
        return await httpClient.GetFromJsonAsync(path, jsonTypeInfo, cancellationToken);
    }
}
