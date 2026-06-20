// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Authentication;

/// <summary>
/// The user identity injected by exe.dev's HTTP proxy.
/// </summary>
/// <param name="UserId">The stable, unique exe.dev user identifier.</param>
/// <param name="Email">The authenticated user's email address.</param>
public sealed record ExeDevUser(string UserId, string Email);