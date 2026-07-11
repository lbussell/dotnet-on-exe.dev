// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LoganBussell.ExeDev;

namespace LoganBussell.ExeDev.Authentication;

/// <summary>
/// Authenticates requests using the identity headers injected by exe.dev's HTTP proxy. The proxy
/// only adds these headers for users it has authenticated, so their presence is sufficient proof
/// of identity. See https://exe.dev/docs/login-with-exe.md.
/// </summary>
/// <remarks>
/// This handler must be used only for traffic that reaches the app through exe.dev's trusted HTTP
/// proxy. Directly reachable apps must prevent clients from supplying the trusted identity headers.
/// </remarks>
public sealed class ExeDevAuthenticationHandler(
    IOptionsMonitor<ExeDevAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<ExeDevAuthenticationOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? userId = Request.Headers[ExeDevAuthenticationDefaults.UserIdHeader];
        string? email = Request.Headers[ExeDevAuthenticationDefaults.EmailHeader];

        // The proxy strips these headers for unauthenticated users, so a missing
        // user id means the request is anonymous (NoResult), not a failure.
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        List<Claim> claims =
        [
            new Claim(ExeDevAuthenticationDefaults.UserIdClaimType, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, email),
        ];

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (!Options.RedirectToLoginOnChallenge)
        {
            return base.HandleChallengeAsync(properties);
        }

        string currentPath = string.Concat(Request.PathBase, Request.Path, Request.QueryString);
        if (string.IsNullOrEmpty(currentPath))
        {
            currentPath = "/";
        }

        string loginUrl = QueryHelpers.AddQueryString(
            ExeDevAuthenticationDefaults.LoginPath,
            ExeDevAuthenticationDefaults.LoginRedirectParameterName,
            currentPath
        );

        Response.Redirect(loginUrl);
        return Task.CompletedTask;
    }
}