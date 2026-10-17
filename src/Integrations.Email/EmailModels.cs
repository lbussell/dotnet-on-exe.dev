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

/// <summary>
/// An email delivered to the exe.dev VM.
/// </summary>
/// <param name="DeliveryId">The opaque Maildir identifier for this delivery.</param>
/// <param name="DeliveredTo">
/// The envelope recipient from exe.dev's injected <c>Delivered-To</c> header.
/// </param>
/// <param name="Content">The complete email content as delivered by exe.dev.</param>
public sealed record ExeDevEmailDelivery(
    string DeliveryId,
    string DeliveredTo,
    ReadOnlyMemory<byte> Content
);

internal sealed record ExeDevEmailResponse(bool Success, string? Error);