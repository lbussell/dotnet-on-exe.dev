// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using LoganBussell.ExeDev;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddExeDev(options => options.RedirectToLoginOnChallenge = true);

var app = builder.Build();

#if DEBUG
// Simulate the exe.dev proxy by injecting placeholder auth headers
app.Use(
    async (context, next) =>
    {
        if (string.IsNullOrEmpty(context.Request.Headers[ExeDevAuthenticationDefaults.UserIdHeader]))
            context.Request.Headers[ExeDevAuthenticationDefaults.UserIdHeader] = "usr-dev-placeholder";

        if (string.IsNullOrEmpty(context.Request.Headers[ExeDevAuthenticationDefaults.EmailHeader]))
            context.Request.Headers[ExeDevAuthenticationDefaults.EmailHeader] = "developer@example.com";

        await next(context);
    }
);
#endif

app.UseAuthentication();
app.UseAuthorization();

// Report the user identified by exe.dev's HTTP proxy headers.
app.MapGet(
    "/",
    (ClaimsPrincipal principal) =>
        !principal.TryGetExeDevUser(out ExeDevUser? user)
            ? new ExeDevUserStatus(Authenticated: false, UserId: null, Email: null)
            : new ExeDevUserStatus(Authenticated: true, UserId: user.UserId, Email: user.Email)
);

// Only reachable by users authenticated through exe.dev. Anonymous requests
// (which the proxy never adds identity headers to) receive a 401 response.
app.MapGet("/me", GetUserResponse).RequireAuthorization();

// Only reachable by the VM owner, identified via the exe.dev Reflection
// integration. Authenticated non-owners receive 403; anonymous requests 401.
app.MapGet("/admin", GetUserResponse).RequireExeDevOwner();

app.Run();

static ExeDevUserStatus GetUserResponse(ClaimsPrincipal principal)
{
    ExeDevUser user = principal.GetExeDevUser();
    return new ExeDevUserStatus(Authenticated: true, UserId: user.UserId, Email: user.Email);
}

record ExeDevUserStatus(bool Authenticated, string? UserId, string? Email);