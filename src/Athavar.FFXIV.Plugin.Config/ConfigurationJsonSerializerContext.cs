// <copyright file="ConfigurationJsonSerializerContext.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    IncludeFields = true,
    GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(AutoSpearConfiguration))]
[JsonSerializable(typeof(CommonConfiguration))]
[JsonSerializable(typeof(CraftQueueConfiguration))]
[JsonSerializable(typeof(DpsConfiguration))]
[JsonSerializable(typeof(DutyHistoryConfiguration))]
[JsonSerializable(typeof(ImporterConfiguration))]
[JsonSerializable(typeof(InstancinatorConfiguration))]
[JsonSerializable(typeof(MacroConfiguration))]
[JsonSerializable(typeof(OpcodeWizardConfiguration))]
[JsonSerializable(typeof(SliceIsRightConfiguration))]
[JsonSerializable(typeof(YesConfiguration))]
public sealed partial class ConfigurationJsonSerializerContext : JsonSerializerContext
{
}