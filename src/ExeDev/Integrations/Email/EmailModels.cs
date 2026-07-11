// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Integrations.Email;

/// <summary>
/// A plain-text email to send through the exe.dev VM email gateway.
/// </summary>
/// <param name="To">The recipient email address.</param>
/// <param name="Subject">The email subject.</param>
/// <param name="Body">The plain-text email body.</param>
/// <param name="ReplyTo">An optional reply-to email address.</param>
/// <param name="InReplyTo">An optional message ID identifying the message being replied to.</param>
/// <param name="References">Optional space-separated message IDs in the email thread.</param>
public sealed record ExeDevEmailMessage(
    string To,
    string Subject,
    string Body,
    string? ReplyTo = null,
    string? InReplyTo = null,
    string? References = null
);

internal sealed record ExeDevEmailResponse(bool Success, string? Error);