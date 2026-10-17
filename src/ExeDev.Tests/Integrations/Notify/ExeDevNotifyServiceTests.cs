// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using System.Text.Json;
using LoganBussell.ExeDev.Integrations.Notify;
using Microsoft.Extensions.DependencyInjection;

namespace ExeDev.Tests.Integrations.Notify;

public sealed class ExeDevNotifyServiceTests
{
    [Fact]
    public async Task SendAsyncPostsDocumentedPayload()
    {
        HttpRequestMessage? capturedRequest = null;
        string? capturedJson = null;
        StubHttpMessageHandler handler = new(
            async (request, cancellationToken) =>
            {
                capturedRequest = request;
                capturedJson = await request.Content!.ReadAsStringAsync(cancellationToken);
                return JsonResponse("""{"success":true,"sent":3}""");
            }
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        int sent = await notify.SendAsync(new ExeDevNotification("Done", "Task finished"));

        Assert.Equal(3, sent);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://notify.int.exe.xyz/", capturedRequest.RequestUri!.AbsoluteUri);
        Assert.Equal("application/json", capturedRequest.Content!.Headers.ContentType!.MediaType);
        Assert.Equal("""{"title":"Done","body":"Task finished"}""", capturedJson);
    }

    [Fact]
    public async Task SendAsyncOmitsOptionalBody()
    {
        string? capturedJson = null;
        StubHttpMessageHandler handler = new(
            async (request, cancellationToken) =>
            {
                capturedJson = await request.Content!.ReadAsStringAsync(cancellationToken);
                return JsonResponse("""{"success":true,"sent":1}""");
            }
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        await notify.SendAsync(new ExeDevNotification("Done"));

        Assert.Equal("""{"title":"Done"}""", capturedJson);
    }

    [Fact]
    public async Task SendAsyncReturnsZeroWhenSentIsMissing()
    {
        StubHttpMessageHandler handler = new(
            (_, _) => Task.FromResult(JsonResponse("""{"success":true}"""))
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        int sent = await notify.SendAsync(new ExeDevNotification("Done"));

        Assert.Equal(0, sent);
    }

    [Fact]
    public async Task SendAsyncPropagatesIntegrationError()
    {
        StubHttpMessageHandler handler = new(
            (_, _) => Task.FromResult(JsonResponse("""{"error":"no devices registered"}"""))
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => notify.SendAsync(new ExeDevNotification("Done"))
        );

        Assert.Contains("no devices registered", exception.Message);
    }

    [Fact]
    public async Task SendAsyncRejectsUnsuccessfulResponse()
    {
        StubHttpMessageHandler handler = new(
            (_, _) => Task.FromResult(JsonResponse("""{"success":false}"""))
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        await Assert.ThrowsAsync<HttpRequestException>(
            () => notify.SendAsync(new ExeDevNotification("Done"))
        );
    }

    [Fact]
    public async Task SendAsyncPropagatesHttpFailure()
    {
        StubHttpMessageHandler handler = new(
            (_, _) =>
                Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = new StringContent(
                            """{"error":"missing required field: title"}""",
                            Encoding.UTF8,
                            "application/json"
                        ),
                    }
                )
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => notify.SendAsync(new ExeDevNotification("Done"))
        );

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("missing required field: title", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SendAsyncRejectsMissingTitle(string? title)
    {
        StubHttpMessageHandler handler = new(
            (_, _) => throw new InvalidOperationException("No request should be sent.")
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => notify.SendAsync(new ExeDevNotification(title!, "Body"))
        );

        Assert.Equal("Title", exception.ParamName);
    }

    [Fact]
    public async Task SendAsyncPropagatesInvalidJson()
    {
        StubHttpMessageHandler handler = new(
            (_, _) => Task.FromResult(JsonResponse("""{"success":"""))
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();

        await Assert.ThrowsAsync<JsonException>(
            () => notify.SendAsync(new ExeDevNotification("Done"))
        );
    }

    [Fact]
    public async Task SendAsyncPropagatesCancellation()
    {
        StubHttpMessageHandler handler = new(
            async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return JsonResponse("""{"success":true,"sent":1}""");
            }
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevNotifyService notify = services.GetRequiredService<IExeDevNotifyService>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => notify.SendAsync(new ExeDevNotification("Done"), cancellation.Token)
        );
    }

    [Fact]
    public void AddExeDevNotifyPreservesExistingHttpClient()
    {
        Uri existingBaseAddress = new("https://example.com/");
        ServiceCollection services = new();
        services.AddHttpClient("Existing", client => client.BaseAddress = existingBaseAddress);
        services.AddExeDevNotify();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IHttpClientFactory httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        using HttpClient existingClient = httpClientFactory.CreateClient("Existing");

        Assert.Equal(existingBaseAddress, existingClient.BaseAddress);
    }

    private static ServiceProvider CreateServices(HttpMessageHandler handler)
    {
        ServiceCollection services = new();
        services.AddExeDevNotify();
        services.ConfigureHttpClientDefaults(
            builder => builder.ConfigurePrimaryHttpMessageHandler(() => handler)
        );
        return services.BuildServiceProvider();
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responseFactory
    ) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            return responseFactory(request, cancellationToken);
        }
    }
}
