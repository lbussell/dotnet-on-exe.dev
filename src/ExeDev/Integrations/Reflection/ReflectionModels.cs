// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace LoganBussell.ExeDev.Integrations.Reflection;

/// <summary>A single integration attached to the VM.</summary>
/// <param name="Name">The integration's name.</param>
/// <param name="Type">The integration's type (for example <c>github</c>).</param>
/// <param name="Help">A help string, often a usage hint or clone command.</param>
/// <param name="Comment">An optional comment describing the integration.</param>
public sealed record ExeDevIntegration(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("help")] string? Help,
    [property: JsonPropertyName("comment")] string? Comment
);

internal sealed record EmailResponse([property: JsonPropertyName("email")] string? Email);

internal sealed record IntegrationsResponse(
    [property: JsonPropertyName("integrations")] IReadOnlyList<ExeDevIntegration>? Integrations
);

internal sealed record TagsResponse([property: JsonPropertyName("tags")] IReadOnlyList<string>? Tags);

internal sealed record CommentResponse([property: JsonPropertyName("comment")] string? Comment);

internal sealed record DefaultPortResponse([property: JsonPropertyName("default_port")] int? DefaultPort);
