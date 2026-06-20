// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace LoganBussell.ExeDev.Authentication;

/// <summary>
/// Options for authenticating requests from exe.dev's HTTP proxy.
/// </summary>
/// <remarks>
/// This scheme trusts identity headers that are meaningful only when the application is reached
/// through exe.dev's trusted HTTP proxy. Do not expose protected endpoints directly to clients that
/// can supply their own <c>X-ExeDev-UserID</c> or <c>X-ExeDev-Email</c> headers.
/// </remarks>
public sealed class ExeDevAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Redirects unauthenticated challenges to exe.dev's login endpoint instead of returning 401.
    /// </summary>
    public bool RedirectToLoginOnChallenge { get; set; }

    /// <summary>
    /// The exe.dev login path used when <see cref="RedirectToLoginOnChallenge"/> is enabled.
    /// </summary>
    public PathString LoginPath { get; set; } = ExeDevAuthenticationDefaults.LoginPath;

    /// <summary>
    /// Query parameter name used to pass the current path to <see cref="LoginPath"/>.
    /// </summary>
    public string RedirectParameterName { get; set; } = ExeDevAuthenticationDefaults.LoginRedirectParameterName;
}