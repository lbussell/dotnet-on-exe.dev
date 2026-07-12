using LoganBussell.ExeDev.Integrations.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddExeDevEmailHandler<LoggingEmailHandler>();

using IHost host = builder.Build();
await host.RunAsync();

internal sealed class LoggingEmailHandler(ILogger<LoggingEmailHandler> logger)
    : IExeDevEmailHandler
{
    public ValueTask HandleAsync(
        ExeDevEmailDelivery delivery,
        CancellationToken cancellationToken = default
    )
    {
        // Content contains the complete raw message for application logic or a MIME parser.
        logger.LogInformation(
            "Received email delivery {DeliveryId} for {DeliveredTo} ({Length} bytes).",
            delivery.DeliveryId,
            delivery.DeliveredTo,
            delivery.Content.Length
        );

        return ValueTask.CompletedTask;
    }
}
