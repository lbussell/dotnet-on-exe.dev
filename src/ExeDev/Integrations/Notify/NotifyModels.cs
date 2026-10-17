// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Integrations.Notify;

/// <summary>
/// A push notification to send to the VM owner's devices through the exe.dev notify integration.
/// </summary>
/// <param name="Title">The notification title.</param>
/// <param name="Body">An optional notification body.</param>
public sealed record ExeDevNotification(string Title, string? Body = null);

internal sealed record ExeDevNotifyResponse(bool Success, int? Sent, string? Error);
