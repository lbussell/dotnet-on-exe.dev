// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace LoganBussell.ExeDev.Integrations.Notify;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
)]
[JsonSerializable(typeof(ExeDevNotification))]
[JsonSerializable(typeof(ExeDevNotifyResponse))]
internal sealed partial class ExeDevNotifyJsonContext : JsonSerializerContext;
