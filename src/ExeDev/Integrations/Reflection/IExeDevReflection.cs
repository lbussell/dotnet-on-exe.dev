// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace LoganBussell.ExeDev.Integrations.Reflection;

/// <summary>
/// Reads VM metadata from the exe.dev Reflection integration, which is
/// auto-attached to every VM at <c>reflection.int.exe.xyz</c>. See
/// https://exe.dev/docs/integrations.md.
/// </summary>
public interface IExeDevReflection
{
    /// <summary>Gets the VM owner's email address, or <c>null</c> if unavailable.</summary>
    Task<string?> GetOwnerEmailAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the integrations available to this VM.</summary>
    Task<IReadOnlyList<ExeDevIntegration>> GetIntegrationsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the tags set on this VM.</summary>
    Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the comment set on this VM, or <c>null</c> if unavailable.</summary>
    Task<string?> GetCommentAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the default proxy port for this VM, or <c>null</c> if unavailable.</summary>
    Task<int?> GetDefaultPortAsync(CancellationToken cancellationToken = default);
}
