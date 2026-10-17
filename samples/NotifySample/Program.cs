using LoganBussell.ExeDev.Integrations.Notify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddExeDevNotify();

using IHost host = builder.Build();
IExeDevNotifyService notify = host.Services.GetRequiredService<IExeDevNotifyService>();

int sent = await notify.SendAsync(
    new ExeDevNotification(
        "dotnet-on-exe.dev notify integration test",
        "This notification was sent by the NotifySample app using IExeDevNotifyService."
    )
);

Console.WriteLine($"The notification was sent to {sent} device(s).");
