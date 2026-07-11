using LoganBussell.ExeDev.Integrations.Email;
using LoganBussell.ExeDev.Integrations.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddExeDevReflection();
builder.Services.AddExeDevEmail();

using IHost host = builder.Build();
IExeDevReflection reflection = host.Services.GetRequiredService<IExeDevReflection>();
IExeDevEmailService email = host.Services.GetRequiredService<IExeDevEmailService>();

string ownerEmail =
    await reflection.GetOwnerEmailAsync()
    ?? throw new InvalidOperationException("The reflection integration did not return an owner email.");

await email.SendAsync(
    new ExeDevEmailMessage(
        ownerEmail,
        "dotnet-on-exe.dev email integration test",
        "This message was sent by the EmailSample app using IExeDevReflection and IExeDevEmailService."
    )
);

Console.WriteLine("The test email was accepted by the exe.dev gateway.");