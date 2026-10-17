// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Integrations.Notify;

/// <summary>
/// Sends push notifications to the VM owner's exe.dev mobile app through the notify integration at
/// <c>notify.int.exe.xyz</c>. The integration must be attached to the VM.
/// </summary>
/// <remarks>
/// HTTP, timeout, cancellation, JSON, and integration errors are propagated to the caller.
/// </remarks>
public interface IExeDevNotifyService
{
    /// <summary>Sends a push notification.</summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">A token for canceling the request.</param>
    /// <returns>The number of devices the notification was sent to.</returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="notification"/> does not have a title.
    /// </exception>
    /// <exception cref="HttpRequestException">
    /// The integration could not be reached or rejected the notification.
    /// </exception>
    Task<int> SendAsync(
        ExeDevNotification notification,
        CancellationToken cancellationToken = default
    );
}
