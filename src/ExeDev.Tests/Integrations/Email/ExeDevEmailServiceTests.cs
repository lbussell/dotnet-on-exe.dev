// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using System.Text.Json;
using LoganBussell.ExeDev.Integrations.Email;
using Microsoft.Extensions.DependencyInjection;

namespace ExeDev.Tests.Integrations.Email;

public sealed class ExeDevEmailServiceTests
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
                return JsonResponse("""{"success":true}""");
            }
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();
        ExeDevEmailMessage message = new(
            "recipient@example.com",
            "Build complete",
            "Your build finished successfully.",
            "reply@example.com",
            "<previous@example.com>",
            "<first@example.com> <previous@example.com>"
        );

        await email.SendAsync(message);

        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal(
            "http://169.254.169.254/gateway/email/send",
            capturedRequest.RequestUri!.AbsoluteUri
        );
        Assert.Equal("application/json", capturedRequest.Content!.Headers.ContentType!.MediaType);
        Assert.Equal(
            """
            {"to":"recipient@example.com","subject":"Build complete","body":"Your build finished successfully.","reply_to":"reply@example.com","in_reply_to":"\u003Cprevious@example.com\u003E","references":"\u003Cfirst@example.com\u003E \u003Cprevious@example.com\u003E"}
            """,
            capturedJson
        );
    }

    [Fact]
    public async Task SendAsyncOmitsOptionalFields()
    {
        string? capturedJson = null;
        StubHttpMessageHandler handler = new(
            async (request, cancellationToken) =>
            {
                capturedJson = await request.Content!.ReadAsStringAsync(cancellationToken);
                return JsonResponse("""{"success":true}""");
            }
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();

        await email.SendAsync(new ExeDevEmailMessage("recipient@example.com", "Subject", "Body"));

        Assert.Equal(
            """{"to":"recipient@example.com","subject":"Subject","body":"Body"}""",
            capturedJson
        );
    }

    [Fact]
    public async Task SendAsyncPropagatesGatewayError()
    {
        StubHttpMessageHandler handler = new(
            (_, _) => Task.FromResult(JsonResponse("""{"error":"recipient is not allowed"}"""))
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => email.SendAsync(new ExeDevEmailMessage("other@example.com", "Subject", "Body"))
        );

        Assert.Contains("recipient is not allowed", exception.Message);
    }

    [Fact]
    public async Task SendAsyncRejectsUnsuccessfulResponse()
    {
        StubHttpMessageHandler handler = new(
            (_, _) => Task.FromResult(JsonResponse("""{"success":false}"""))
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();

        await Assert.ThrowsAsync<HttpRequestException>(
            () => email.SendAsync(new ExeDevEmailMessage("owner@example.com", "Subject", "Body"))
        );
    }

    [Fact]
    public async Task SendAsyncPropagatesHttpFailure()
    {
        StubHttpMessageHandler handler = new(
            (_, _) =>
                Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                    {
                        Content = new StringContent(
                            """{"error":"rate limited"}""",
                            Encoding.UTF8,
                            "application/json"
                        ),
                    }
                )
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();

        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => email.SendAsync(new ExeDevEmailMessage("owner@example.com", "Subject", "Body"))
        );

        Assert.Equal(HttpStatusCode.TooManyRequests, exception.StatusCode);
        Assert.Contains("rate limited", exception.Message);
    }

    [Theory]
    [InlineData(null, "Subject", "Body", "To")]
    [InlineData("owner@example.com", null, "Body", "Subject")]
    [InlineData("owner@example.com", "Subject", null, "Body")]
    [InlineData("", "Subject", "Body", "To")]
    public async Task SendAsyncRejectsMissingRequiredField(
        string? to,
        string? subject,
        string? body,
        string expectedParameterName
    )
    {
        StubHttpMessageHandler handler = new(
            (_, _) => throw new InvalidOperationException("No request should be sent.")
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => email.SendAsync(new ExeDevEmailMessage(to!, subject!, body!))
        );

        Assert.Equal(expectedParameterName, exception.ParamName);
    }

    [Fact]
    public async Task SendAsyncPropagatesInvalidJson()
    {
        StubHttpMessageHandler handler = new(
            (_, _) => Task.FromResult(JsonResponse("""{"success":"""))
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();

        await Assert.ThrowsAsync<JsonException>(
            () => email.SendAsync(new ExeDevEmailMessage("owner@example.com", "Subject", "Body"))
        );
    }

    [Fact]
    public async Task SendAsyncPropagatesCancellation()
    {
        StubHttpMessageHandler handler = new(
            async (_, cancellationToken) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return JsonResponse("""{"success":true}""");
            }
        );
        await using ServiceProvider services = CreateServices(handler);
        IExeDevEmailService email = services.GetRequiredService<IExeDevEmailService>();
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () =>
                email.SendAsync(
                    new ExeDevEmailMessage("owner@example.com", "Subject", "Body"),
                    cancellation.Token
                )
        );
    }

    [Fact]
    public void AddExeDevEmailPreservesExistingHttpClient()
    {
        Uri existingBaseAddress = new("https://example.com/");
        ServiceCollection services = new();
        services.AddHttpClient("Existing", client => client.BaseAddress = existingBaseAddress);
        services.AddExeDevEmail();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IHttpClientFactory httpClientFactory =
            serviceProvider.GetRequiredService<IHttpClientFactory>();

        using HttpClient existingClient = httpClientFactory.CreateClient("Existing");

        Assert.Equal(existingBaseAddress, existingClient.BaseAddress);
    }

    private static ServiceProvider CreateServices(HttpMessageHandler handler)
    {
        ServiceCollection services = new();
        services.AddExeDevEmail();
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