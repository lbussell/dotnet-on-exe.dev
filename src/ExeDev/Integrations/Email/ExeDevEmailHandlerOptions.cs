// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Integrations.Email;

/// <summary>
/// Configures exe.dev email delivery handling.
/// </summary>
public sealed class ExeDevEmailHandlerOptions
{
    /// <summary>
    /// Gets or sets the Maildir root containing the <c>new</c> and <c>cur</c> directories.
    /// </summary>
    public string MaildirPath { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Maildir");

    /// <summary>Gets or sets how frequently the Maildir is checked for new deliveries.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
}
