// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;

namespace LoganBussell.ExeDev.Integrations.Notify;

internal sealed class ExeDevNotifyService(IHttpClientFactory httpClientFactory)
    : IExeDevNotifyService
{
    internal const string HttpClientName = "LoganBussell.ExeDev.Integrations.Notify";
    internal static readonly Uri BaseAddress = new("https://notify.int.exe.xyz/");
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public async Task<int> SendAsync(
        ExeDevNotification notification,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(notification);
        if (string.IsNullOrEmpty(notification.Title))
        {
            throw new ArgumentException(
                "The notification title is required.",
                nameof(notification.Title)
            );
        }

        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "",
            notification,
            ExeDevNotifyJsonContext.Default.ExeDevNotification,
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                string.IsNullOrEmpty(error)
                    ? "The exe.dev notify integration rejected the notification."
                    : $"The exe.dev notify integration rejected the notification: {error}",
                null,
                response.StatusCode
            );
        }

        ExeDevNotifyResponse? result = await response.Content.ReadFromJsonAsync(
            ExeDevNotifyJsonContext.Default.ExeDevNotifyResponse,
            cancellationToken
        );
        if (!string.IsNullOrEmpty(result?.Error))
        {
            throw new HttpRequestException(
                $"The exe.dev notify integration rejected the notification: {result.Error}"
            );
        }

        if (result?.Success != true)
        {
            throw new HttpRequestException(
                "The exe.dev notify integration returned an unsuccessful response."
            );
        }

        return result.Sent ?? 0;
    }
}
