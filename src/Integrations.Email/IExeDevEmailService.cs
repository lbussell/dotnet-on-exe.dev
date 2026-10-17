// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Integrations.Email;

/// <summary>
/// Sends plain-text email through the exe.dev VM email gateway. See
/// https://exe.dev/docs/send-email.md.
/// </summary>
/// <remarks>
/// The gateway limits recipients and sending frequency. HTTP, timeout, cancellation, JSON, and
/// gateway errors are propagated to the caller.
/// </remarks>
public interface IExeDevEmailService
{
    /// <summary>Sends an email.</summary>
    /// <param name="message">The email to send.</param>
    /// <param name="cancellationToken">A token for canceling the request.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="message"/> does not contain all required fields.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// The gateway could not be reached or rejected the email.
    /// </exception>
    Task SendAsync(ExeDevEmailMessage message, CancellationToken cancellationToken = default);
}