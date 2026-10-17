// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Integrations.Email;

/// <summary>
/// Handles emails delivered to the exe.dev VM.
/// </summary>
/// <remarks>
/// A new dependency injection scope and handler instance are created for each delivery. Returning
/// successfully marks the delivery as handled. Throwing an exception leaves it pending for retry,
/// so implementations should use <see cref="ExeDevEmailDelivery.DeliveryId"/> for idempotency when
/// necessary.
/// </remarks>
public interface IExeDevEmailHandler
{
    /// <summary>Handles an email delivery.</summary>
    /// <param name="delivery">The email delivered by exe.dev.</param>
    /// <param name="cancellationToken">A token that is canceled when the host is stopping.</param>
    ValueTask HandleAsync(
        ExeDevEmailDelivery delivery,
        CancellationToken cancellationToken = default
    );
}
