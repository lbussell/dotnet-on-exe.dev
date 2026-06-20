// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoganBussell.ExeDev.Authentication;

/// <summary>
/// Authenticates requests using the identity headers injected by exe.dev's HTTP proxy. The proxy
/// only adds these headers for users it has authenticated, so their presence is sufficient proof
/// of identity. See https://exe.dev/docs/login-with-exe.md.
/// </summary>
public sealed class ExeDevAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
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

        var claim = new Claim(ClaimTypes.NameIdentifier, userId);
        List<Claim> claims = [claim];

        if (!string.IsNullOrEmpty(email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
            claims.Add(new Claim(ClaimTypes.Name, email));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
