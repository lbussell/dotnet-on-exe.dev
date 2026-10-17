// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoganBussell.ExeDev.Integrations.Email;

internal sealed class ExeDevEmailHandlerBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<ExeDevEmailHandlerOptions> options,
    ILogger<ExeDevEmailHandlerBackgroundService> logger
) : BackgroundService
{
    private readonly string _newDirectory = Path.Combine(options.Value.MaildirPath, "new");
    private readonly string _currentDirectory = Path.Combine(options.Value.MaildirPath, "cur");
    private readonly TimeSpan _pollingInterval = options.Value.PollingInterval;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(_newDirectory);
        Directory.CreateDirectory(_currentDirectory);

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessPendingDeliveriesAsync(stoppingToken);
            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingDeliveriesAsync(CancellationToken cancellationToken)
    {
        string[] paths = Directory
            .EnumerateFiles(_newDirectory, "*", SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal)
            .ToArray();

        foreach (string path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ProcessDeliveryAsync(path, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (FileNotFoundException)
            {
                logger.LogDebug(
                    "Email delivery {DeliveryId} was removed before it could be handled.",
                    Path.GetFileName(path)
                );
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Email delivery {DeliveryId} could not be handled and will be retried.",
                    Path.GetFileName(path)
                );
            }
        }
    }

    private async Task ProcessDeliveryAsync(string path, CancellationToken cancellationToken)
    {
        byte[] content = await File.ReadAllBytesAsync(path, cancellationToken);
        if (!TryGetDeliveredTo(content, out string deliveredTo))
        {
            throw new InvalidDataException(
                "The email does not begin with exe.dev's Delivered-To header."
            );
        }

        ExeDevEmailDelivery delivery = new(Path.GetFileName(path), deliveredTo, content);
        await using (AsyncServiceScope scope = serviceScopeFactory.CreateAsyncScope())
        {
            IExeDevEmailHandler handler =
                scope.ServiceProvider.GetRequiredService<IExeDevEmailHandler>();
            await handler.HandleAsync(delivery, cancellationToken);
        }

        File.Move(path, Path.Combine(_currentDirectory, Path.GetFileName(path)));
    }

    private static bool TryGetDeliveredTo(
        ReadOnlySpan<byte> content,
        out string deliveredTo
    )
    {
        int lineFeedIndex = content.IndexOf((byte)'\n');
        ReadOnlySpan<byte> firstLine =
            lineFeedIndex >= 0 ? content[..lineFeedIndex] : content;
        if (!firstLine.IsEmpty && firstLine[^1] == '\r')
        {
            firstLine = firstLine[..^1];
        }

        string header = Encoding.UTF8.GetString(firstLine);
        int separatorIndex = header.IndexOf(':');
        if (
            separatorIndex < 0
            || !header[..separatorIndex].Equals(
                "Delivered-To",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            deliveredTo = string.Empty;
            return false;
        }

        deliveredTo = header[(separatorIndex + 1)..].Trim();
        return deliveredTo.Length > 0;
    }
}
