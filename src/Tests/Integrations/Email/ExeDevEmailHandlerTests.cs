// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using System.Threading.Channels;
using LoganBussell.ExeDev.Integrations.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExeDev.Tests.Integrations.Email;

public sealed class ExeDevEmailHandlerTests
{
    [Fact]
    public async Task HandlesPendingDeliveryAndMovesItToCurrent()
    {
        using TemporaryMaildir maildir = new();
        const string deliveryId = "delivery-1";
        byte[] content = Encoding.UTF8.GetBytes(
            "Delivered-To: contact@sample.exe.xyz\r\n"
                + "To: another@example.com\r\n"
                + "Subject: Hello\r\n"
                + "\r\n"
                + "Message body"
        );
        maildir.WriteNew(deliveryId, content);
        HandlerState state = new();
        await using ServiceProvider services = CreateServices(maildir, state);
        IHostedService worker = services.GetRequiredService<IHostedService>();

        await worker.StartAsync(CancellationToken.None);
        try
        {
            HandledDelivery handled = await state.ReadAsync();
            await WaitForFileAsync(maildir.CurrentPath(deliveryId));

            Assert.Equal(deliveryId, handled.Delivery.DeliveryId);
            Assert.Equal("contact@sample.exe.xyz", handled.Delivery.DeliveredTo);
            Assert.Equal(content, handled.Delivery.Content.ToArray());
            Assert.False(File.Exists(maildir.NewPath(deliveryId)));
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task RetriesDeliveryAfterHandlerFailure()
    {
        using TemporaryMaildir maildir = new();
        const string deliveryId = "delivery-2";
        maildir.WriteNew(
            deliveryId,
            Encoding.UTF8.GetBytes("Delivered-To: owner@sample.exe.xyz\n\nRetry me")
        );
        HandlerState state = new() { FailuresBeforeSuccess = 1 };
        await using ServiceProvider services = CreateServices(maildir, state);
        IHostedService worker = services.GetRequiredService<IHostedService>();

        await worker.StartAsync(CancellationToken.None);
        try
        {
            await state.ReadAsync();
            await WaitForFileAsync(maildir.CurrentPath(deliveryId));

            Assert.Equal(2, state.Attempts);
            Assert.False(File.Exists(maildir.NewPath(deliveryId)));
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task CreatesHandlerScopeForEachDelivery()
    {
        using TemporaryMaildir maildir = new();
        maildir.WriteNew(
            "delivery-3",
            Encoding.UTF8.GetBytes("Delivered-To: one@sample.exe.xyz\n\nFirst")
        );
        maildir.WriteNew(
            "delivery-4",
            Encoding.UTF8.GetBytes("Delivered-To: two@sample.exe.xyz\n\nSecond")
        );
        HandlerState state = new();
        await using ServiceProvider services = CreateServices(maildir, state);
        IHostedService worker = services.GetRequiredService<IHostedService>();

        await worker.StartAsync(CancellationToken.None);
        try
        {
            HandledDelivery first = await state.ReadAsync();
            HandledDelivery second = await state.ReadAsync();

            Assert.NotEqual(first.HandlerId, second.HandlerId);
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task LeavesDeliveryPendingWhenHostStopsDuringHandling()
    {
        using TemporaryMaildir maildir = new();
        const string deliveryId = "delivery-5";
        maildir.WriteNew(
            deliveryId,
            Encoding.UTF8.GetBytes("Delivered-To: owner@sample.exe.xyz\n\nWait")
        );
        HandlerState state = new() { WaitForCancellation = true };
        await using ServiceProvider services = CreateServices(maildir, state);
        IHostedService worker = services.GetRequiredService<IHostedService>();

        await worker.StartAsync(CancellationToken.None);
        await state.ReadAsync();
        await worker.StopAsync(CancellationToken.None);

        Assert.True(File.Exists(maildir.NewPath(deliveryId)));
        Assert.False(File.Exists(maildir.CurrentPath(deliveryId)));
    }

    [Fact]
    public async Task LeavesDeliveryWithoutInjectedHeaderPending()
    {
        using TemporaryMaildir maildir = new();
        const string deliveryId = "delivery-6";
        maildir.WriteNew(
            deliveryId,
            Encoding.UTF8.GetBytes("To: owner@sample.exe.xyz\n\nMissing Delivered-To")
        );
        HandlerState state = new();
        await using ServiceProvider services = CreateServices(maildir, state);
        IHostedService worker = services.GetRequiredService<IHostedService>();

        await worker.StartAsync(CancellationToken.None);
        try
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            Assert.Equal(0, state.Attempts);
            Assert.True(File.Exists(maildir.NewPath(deliveryId)));
        }
        finally
        {
            await worker.StopAsync(CancellationToken.None);
        }
    }

    private static ServiceProvider CreateServices(
        TemporaryMaildir maildir,
        HandlerState state
    )
    {
        ServiceCollection services = new();
        services.AddSingleton(state);
        services.AddExeDevEmailHandler<RecordingEmailHandler>(options =>
        {
            options.MaildirPath = maildir.Root;
            options.PollingInterval = TimeSpan.FromMilliseconds(10);
        });
        return services.BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true }
        );
    }

    private static async Task WaitForFileAsync(string path)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
        while (!File.Exists(path))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
        }
    }

    private sealed class RecordingEmailHandler(HandlerState state) : IExeDevEmailHandler
    {
        private readonly Guid _id = Guid.NewGuid();

        public ValueTask HandleAsync(
            ExeDevEmailDelivery delivery,
            CancellationToken cancellationToken = default
        )
        {
            return state.HandleAsync(delivery, _id, cancellationToken);
        }
    }

    private sealed class HandlerState
    {
        private readonly Channel<HandledDelivery> _deliveries =
            Channel.CreateUnbounded<HandledDelivery>();
        private int _attempts;

        public int FailuresBeforeSuccess { get; init; }

        public bool WaitForCancellation { get; init; }

        public int Attempts => Volatile.Read(ref _attempts);

        public async ValueTask HandleAsync(
            ExeDevEmailDelivery delivery,
            Guid handlerId,
            CancellationToken cancellationToken
        )
        {
            int attempt = Interlocked.Increment(ref _attempts);
            if (attempt <= FailuresBeforeSuccess)
            {
                throw new InvalidOperationException("Simulated handler failure.");
            }

            await _deliveries.Writer.WriteAsync(
                new HandledDelivery(delivery, handlerId),
                cancellationToken
            );
            if (WaitForCancellation)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
        }

        public Task<HandledDelivery> ReadAsync()
        {
            return _deliveries
                .Reader.ReadAsync()
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(5));
        }
    }

    private sealed record HandledDelivery(ExeDevEmailDelivery Delivery, Guid HandlerId);

    private sealed class TemporaryMaildir : IDisposable
    {
        public TemporaryMaildir()
        {
            Root = Path.Combine(Path.GetTempPath(), $"exedev-email-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path.Combine(Root, "new"));
            Directory.CreateDirectory(Path.Combine(Root, "cur"));
        }

        public string Root { get; }

        public string NewPath(string deliveryId) => Path.Combine(Root, "new", deliveryId);

        public string CurrentPath(string deliveryId) => Path.Combine(Root, "cur", deliveryId);

        public void WriteNew(string deliveryId, byte[] content)
        {
            File.WriteAllBytes(NewPath(deliveryId), content);
        }

        public void Dispose()
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}