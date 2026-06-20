// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;
using LoganBussell.ExeDev;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default);
});

builder.Services.AddExeDev(options =>
{
    options.RedirectToLoginOnChallenge = true;
});

var app = builder.Build();

// In DEBUG builds, simulate the exe.dev proxy by injecting placeholder auth
// headers (only when they are not already present).
#if DEBUG
app.AddExeDevDebugUser();
#endif

app.UseAuthentication();
app.UseAuthorization();

// Report the user identified by exe.dev's HTTP proxy headers.
app.MapGet("/", GetUserStatus);

// Only reachable by users authenticated through exe.dev. Anonymous requests
// (which the proxy never adds identity headers to) receive a 401 response.
app.MapGet("/me", GetUserResponse).RequireAuthorization();

// Only reachable by the VM owner, identified via the exe.dev Reflection
// integration. Authenticated non-owners receive 403; anonymous requests 401.
app.MapGet("/admin", GetUserResponse).RequireExeDevOwner();

app.Run();

static ExeDevUserStatus GetUserStatus(HttpContext context)
{
    if (!context.User.TryGetExeDevUser(out ExeDevUser? user))
    {
        return new ExeDevUserStatus(Authenticated: false, UserId: null, Email: null);
    }

    return new ExeDevUserStatus(Authenticated: true, UserId: user.UserId, Email: user.Email);
}

static ExeDevUserResponse GetUserResponse(HttpContext context)
{
    ExeDevUser user = context.User.GetExeDevUser();

    return new ExeDevUserResponse(
        Authenticated: true,
        UserId: user.UserId,
        Email: user.Email
    );
}

internal record ExeDevUserStatus(bool Authenticated, string? UserId, string? Email);

internal record ExeDevUserResponse(bool Authenticated, string UserId, string Email);

[JsonSerializable(typeof(ExeDevUserStatus))]
[JsonSerializable(typeof(ExeDevUserResponse))]
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