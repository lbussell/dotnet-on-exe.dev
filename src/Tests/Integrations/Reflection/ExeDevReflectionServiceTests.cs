// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using System.Text.Json;
using LoganBussell.ExeDev.Integrations.Reflection;

namespace ExeDev.Tests.Integrations.Reflection;

public sealed class ExeDevReflectionServiceTests
{
    [Fact]
    public async Task GetOwnerEmailAsyncPropagatesHttpFailure()
    {
        using HttpClient httpClient = CreateHttpClient(
            _ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        );
        ExeDevReflectionService service = new(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetOwnerEmailAsync());
    }

    [Fact]
    public async Task GetOwnerEmailAsyncPropagatesInvalidJson()
    {
        using HttpClient httpClient = CreateHttpClient(
            _ =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"email":""", Encoding.UTF8, "application/json"),
                }
        );
        ExeDevReflectionService service = new(httpClient);

        await Assert.ThrowsAsync<JsonException>(() => service.GetOwnerEmailAsync());
    }

    private static HttpClient CreateHttpClient(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        return new HttpClient(new StubHttpMessageHandler(responseFactory))
        {
            BaseAddress = ExeDevReflectionService.BaseAddress,
        };
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            return Task.FromResult(responseFactory(request));
        }
    }
}
