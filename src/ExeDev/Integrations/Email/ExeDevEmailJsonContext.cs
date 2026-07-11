// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace LoganBussell.ExeDev.Integrations.Email;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ExeDevEmailMessage))]
[JsonSerializable(typeof(ExeDevEmailResponse))]
internal sealed partial class ExeDevEmailJsonContext : JsonSerializerContext;