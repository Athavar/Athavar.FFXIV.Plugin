// <copyright file="CommonJsonSerializerContext.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Definitions;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(JobDefinition))]
public partial class CommonJsonSerializerContext : JsonSerializerContext
{
}