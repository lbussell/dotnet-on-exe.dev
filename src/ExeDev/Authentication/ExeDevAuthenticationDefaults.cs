// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Authentication;

/// <summary>
/// Default values describing how exe.dev's HTTP proxy communicates the
/// authenticated user to the application. See https://exe.dev/docs/login-with-exe.md.
/// </summary>
public static class ExeDevAuthenticationDefaults
{
    /// <summary>Name of the authentication scheme backed by exe.dev proxy headers.</summary>
    public const string AuthenticationScheme = "ExeDev";

    /// <summary>Header carrying a stable, unique user identifier.</summary>
    public const string UserIdHeader = "X-ExeDev-UserID";

    /// <summary>Header carrying the user's email address.</summary>
    public const string EmailHeader = "X-ExeDev-Email";

    /// <summary>Path for exe.dev's hosted login endpoint.</summary>
    public const string LoginPath = "/__exe.dev/login";

    /// <summary>Query parameter used by exe.dev's login endpoint for the post-login return path.</summary>
    public const string LoginRedirectParameterName = "redirect";
}