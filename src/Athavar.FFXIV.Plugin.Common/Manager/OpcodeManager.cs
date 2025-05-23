﻿// <copyright file="OpcodeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Text.Json;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using IDefinitionManager = Athavar.FFXIV.Plugin.Common.Manager.Interface.IDefinitionManager;

internal sealed class OpcodeManager : IOpcodeManager
{
    private readonly IPluginLogger logger;
    private readonly IDalamudServices dalamudServices;
    private readonly OpcodeWizardConfiguration configuration;
    private readonly IDefinitionManager definitionManager;

    private readonly Dictionary<ushort, Opcode> opcodes = new();

    public OpcodeManager(IDalamudServices dalamudServices, OpcodeWizardConfiguration configuration, IDefinitionManager definitionManager)
    {
        this.logger = dalamudServices.PluginLogger;
        this.dalamudServices = dalamudServices;
        this.configuration = configuration;
        this.definitionManager = definitionManager;
        _ = this.Populate();
    }

    public IReadOnlyDictionary<ushort, Opcode> Opcodes => this.opcodes;

    public ushort GetOpcode(Opcode opcode) => this.opcodes.Where(op => op.Value == opcode).Select(op => op.Key).FirstOrDefault();

    public void AddOrUpdate(Opcode opcode, ushort value)
    {
        if (this.configuration.Opcodes.TryGetValue(opcode, out var current))
        {
            this.opcodes.Remove(current);
            this.configuration.Opcodes[opcode] = value;
        }
        else
        {
            this.configuration.Opcodes.TryAdd(opcode, value);
        }

        this.opcodes.TryAdd(value, opcode);
        this.configuration.Save();
    }

    public void Remove(Opcode opcode)
    {
        if (this.configuration.Opcodes.TryGetValue(opcode, out var current))
        {
            this.configuration.Opcodes.Remove(opcode);
            this.opcodes.Remove(current);
            this.configuration.Save();
        }
    }

    public async Task CheckRemote()
    {
        if (!this.configuration.RemoteUpdate)
        {
            try
            {
                var updateConfigString = await this.dalamudServices.HttpClient.GetStringAsync("https://raw.githubusercontent.com/Athavar/Athavar.FFXIV.DalaRepo/master/opcodes.json");
                var updateConfig = JsonSerializer.Deserialize<UpdateConfig>(updateConfigString);
                if (updateConfig?.GameVersion == this.configuration.GameVersion)
                {
                    foreach (var (key, value) in updateConfig.Opcodes)
                    {
                        this.configuration.Opcodes.TryAdd(key, value);
                        this.Add(key, value);
                    }

                    this.logger.Information("Opcodes updated from Remote");

                    this.configuration.RemoteUpdate = true;
                    this.configuration.Save();
                }
            }
            catch (Exception ex)
            {
                this.logger.Error(ex, "Fail to update Opcodes from Remote");
            }
        }
    }

    private async Task Populate()
    {
        if (this.configuration.GameVersion != this.definitionManager.StartInfo.GameVersion?.ToString())
        {
            // reset
            this.configuration.Opcodes.Clear();
            this.configuration.RemoteUpdate = false;
            this.configuration.GameVersion = this.definitionManager.StartInfo.GameVersion?.ToString() ?? string.Empty;
            this.configuration.Save();
        }

        await this.CheckRemote();

        foreach (var (opcode, value) in this.configuration.Opcodes)
        {
            this.Add(opcode, value);
        }

        /*
        Machina.FFXIV.Headers.Opcodes.OpcodeManager machinaManager = new();
        machinaManager.SetRegion(GameRegion.Global);
        var codes = machinaManager.CurrentOpcodes;

        // Add(Opcode.StatusEffectListBozja, codes["StatusEffectList2"]);
        // Add(Opcode.StatusEffectListPlayer, codes["StatusEffectList3"]);
        // Add(Opcode.StatusEffectListDouble, codes["BossStatusEffectList"]);
        // Add(Opcode.SpawnBoss, codes["NpcSpawn2"]);
        */
    }

    private void Add(Opcode opcode, ushort code) => this.opcodes.TryAdd(code, opcode);

    private class UpdateConfig
    {
        [JsonPropertyName("Opcodes")]
        public Dictionary<Opcode, ushort> Opcodes { get; set; } = new();

        [JsonPropertyName("GameVersion")]
        public string GameVersion { get; set; } = string.Empty;
    }
}