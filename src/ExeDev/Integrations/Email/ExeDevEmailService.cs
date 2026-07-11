// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;

namespace LoganBussell.ExeDev.Integrations.Email;

internal sealed class ExeDevEmailService(IHttpClientFactory httpClientFactory)
    : IExeDevEmailService
{
    internal const string HttpClientName = "LoganBussell.ExeDev.Email";
    internal static readonly Uri BaseAddress = new("http://169.254.169.254/");
    internal static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    public async Task SendAsync(
        ExeDevEmailMessage message,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(message);

        ValidateRequiredField(message.To, nameof(message.To));
        ValidateRequiredField(message.Subject, nameof(message.Subject));
        ValidateRequiredField(message.Body, nameof(message.Body));

        using HttpClient httpClient = httpClientFactory.CreateClient(HttpClientName);
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(
            "gateway/email/send",
            message,
            ExeDevEmailJsonContext.Default.ExeDevEmailMessage,
            cancellationToken
        );
        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                string.IsNullOrEmpty(error)
                    ? "The exe.dev email gateway rejected the message."
                    : $"The exe.dev email gateway rejected the message: {error}",
                null,
                response.StatusCode
            );
        }

        ExeDevEmailResponse? result = await response.Content.ReadFromJsonAsync(
            ExeDevEmailJsonContext.Default.ExeDevEmailResponse,
            cancellationToken
        );
        if (!string.IsNullOrEmpty(result?.Error))
        {
            throw new HttpRequestException(
                $"The exe.dev email gateway rejected the message: {result.Error}"
            );
        }

        if (result?.Success != true)
        {
            throw new HttpRequestException(
                "The exe.dev email gateway returned an unsuccessful response."
            );
        }
    }

    private static void ValidateRequiredField(string? value, string fieldName)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("The email field is required.", fieldName);
        }
    }
}