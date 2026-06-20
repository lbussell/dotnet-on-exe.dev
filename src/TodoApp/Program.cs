// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using System.Text.Json.Serialization;
using LoganBussell.ExeDev.Authentication;
using LoganBussell.ExeDev.Authorization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

builder.Services.AddExeDevAuthentication();
builder.Services.AddExeDevOwnerPolicy();

var app = builder.Build();

// In DEBUG builds, simulate the exe.dev proxy by injecting placeholder auth
// headers (only when they are not already present).
#if DEBUG
app.AddExeDevDebugUser();
#endif

app.UseAuthentication();
app.UseAuthorization();

// Report the user identified by exe.dev's HTTP proxy headers.
app.MapGet(
    "/",
    (HttpContext context) =>
    {
        string? userId = context.Request.Headers[ExeDevAuthenticationDefaults.UserIdHeader];
        string? email = context.Request.Headers[ExeDevAuthenticationDefaults.EmailHeader];

        return new ExeDevUser(Authenticated: !string.IsNullOrEmpty(userId), UserId: userId, Email: email);
    }
);

// Only reachable by users authenticated through exe.dev. Anonymous requests
// (which the proxy never adds identity headers to) receive a 401 response.
app.MapGet(
        "/me",
        (HttpContext context) =>
        {
            var user = context.User;

            return new ExeDevUser(
                Authenticated: true,
                UserId: user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email: user.FindFirst(ClaimTypes.Email)?.Value
            );
        }
    )
    .RequireAuthorization();

// Only reachable by the VM owner, identified via the exe.dev Reflection
// integration. Authenticated non-owners receive 403; anonymous requests 401.
app.MapGet(
        "/admin",
        (HttpContext context) =>
        {
            var user = context.User;

            return new ExeDevUser(
                Authenticated: true,
                UserId: user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email: user.FindFirst(ClaimTypes.Email)?.Value
            );
        }
    )
    .RequireExeDevOwner();

app.Run();

internal record ExeDevUser(bool Authenticated, string? UserId, string? Email);

[JsonSerializable(typeof(ExeDevUser))]
internal partial class AppJsonContext : JsonSerializerContext;

static class DebugUserExtensions
{
    public static IApplicationBuilder AddExeDevDebugUser(this IApplicationBuilder app)
    {
        return app.Use(
            async (context, next) =>
            {
                if (string.IsNullOrEmpty(context.Request.Headers[ExeDevAuthenticationDefaults.UserIdHeader]))
                    context.Request.Headers[ExeDevAuthenticationDefaults.UserIdHeader] = "usr-dev-placeholder";

                if (string.IsNullOrEmpty(context.Request.Headers[ExeDevAuthenticationDefaults.EmailHeader]))
                    context.Request.Headers[ExeDevAuthenticationDefaults.EmailHeader] = "developer@example.com";

                await next(context);
            }
        );
    }
}
