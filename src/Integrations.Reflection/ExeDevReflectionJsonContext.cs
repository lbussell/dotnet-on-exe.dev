// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace LoganBussell.ExeDev.Integrations.Reflection;

[JsonSerializable(typeof(EmailResponse))]
[JsonSerializable(typeof(IntegrationsResponse))]
[JsonSerializable(typeof(TagsResponse))]
[JsonSerializable(typeof(CommentResponse))]
[JsonSerializable(typeof(DefaultPortResponse))]
internal sealed partial class ExeDevReflectionJsonContext : JsonSerializerContext;
