using System.Text.Json.Serialization;
using TodoApp.Authentication;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

var app = builder.Build();

// In DEBUG builds, simulate the exe.dev proxy by injecting placeholder auth
// headers (only when they are not already present).
#if DEBUG
app.Use(async (context, next) =>
{
    if (string.IsNullOrEmpty(context.Request.Headers[ExeDevAuthentication.UserIdHeader]))
    {
        context.Request.Headers[ExeDevAuthentication.UserIdHeader] = "usr-dev-placeholder";
    }

    if (string.IsNullOrEmpty(context.Request.Headers[ExeDevAuthentication.EmailHeader]))
    {
        context.Request.Headers[ExeDevAuthentication.EmailHeader] = "developer@example.com";
    }

    await next(context);
});
#endif

// Report the user identified by exe.dev's HTTP proxy headers.
app.MapGet("/", (HttpContext context) =>
{
    string? userId = context.Request.Headers[ExeDevAuthentication.UserIdHeader];
    string? email = context.Request.Headers[ExeDevAuthentication.EmailHeader];

    return new ExeDevUser(
        Authenticated: !string.IsNullOrEmpty(userId),
        UserId: userId,
        Email: email);
});

app.Run();

internal record ExeDevUser(bool Authenticated, string? UserId, string? Email);

[JsonSerializable(typeof(ExeDevUser))]
internal partial class AppJsonContext : JsonSerializerContext;
