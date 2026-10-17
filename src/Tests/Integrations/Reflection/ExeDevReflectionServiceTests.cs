// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using System.Text.Json;
using LoganBussell.ExeDev.Integrations.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ExeDev.Tests.Integrations.Reflection;

public sealed class ExeDevReflectionServiceTests
{
    [Fact]
    public async Task GetOwnerEmailAsyncReadsEmailEndpoint()
    {
        HttpRequestMessage? capturedRequest = null;
        StubHttpMessageHandler handler = new(request =>
        {
            capturedRequest = request;
            return JsonResponse("""{"email":"owner@example.com"}""");
        });
        await using ServiceProvider services = CreateServices(handler);
        IExeDevReflection reflection = services.GetRequiredService<IExeDevReflection>();

        string? email = await reflection.GetOwnerEmailAsync();

        Assert.Equal("owner@example.com", email);
        Assert.Equal(HttpMethod.Get, capturedRequest!.Method);
        Assert.Equal("https://reflection.int.exe.xyz/email", capturedRequest.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetOwnerEmailAsyncPropagatesHttpFailure()
    {
        StubHttpMessageHandler handler = new(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
        await using ServiceProvider services = CreateServices(handler);
        IExeDevReflection reflection = services.GetRequiredService<IExeDevReflection>();

        await Assert.ThrowsAsync<HttpRequestException>(() => reflection.GetOwnerEmailAsync());
    }

    [Fact]
    public async Task GetOwnerEmailAsyncPropagatesInvalidJson()
    {
        StubHttpMessageHandler handler = new(_ => JsonResponse("""{"email":"""));
        await using ServiceProvider services = CreateServices(handler);
        IExeDevReflection reflection = services.GetRequiredService<IExeDevReflection>();

        await Assert.ThrowsAsync<JsonException>(() => reflection.GetOwnerEmailAsync());
    }

    [Fact]
    public void AddExeDevReflectionPreservesExistingHttpClient()
    {
        Uri existingBaseAddress = new("https://example.com/");
        ServiceCollection services = new();
        services.AddHttpClient("Existing", client => client.BaseAddress = existingBaseAddress);
        services.AddExeDevReflection();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        using HttpClient existingClient = httpClientFactory.CreateClient("Existing");

        Assert.Equal(existingBaseAddress, existingClient.BaseAddress);
    }

    private static ServiceProvider CreateServices(HttpMessageHandler handler)
    {
        ServiceCollection services = new();
        services.AddExeDevReflection();
        services.ConfigureHttpClientDefaults(builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler));
        return services.BuildServiceProvider();
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
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
