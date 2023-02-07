// <copyright file="OpcodeWizardConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;

public class OpcodeWizardConfiguration : BasicModuleConfig
{
    [JsonPropertyName("Opcodes")]
    public Dictionary<Opcode, ushort> Opcodes { get; } = new();

    [JsonPropertyName("GameVersion")]
    public string GameVersion { get; set; } = string.Empty;

    public bool RemoteUpdate { get; set; }
}