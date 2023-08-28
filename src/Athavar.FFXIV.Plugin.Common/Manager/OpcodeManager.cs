// <copyright file="OpcodeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Networking.Http;

internal sealed class OpcodeManager : IOpcodeManager
{
    private readonly IPluginLogger logger;
    private readonly OpcodeWizardConfiguration configuration;
    private readonly IDefinitionManager definitionManager;

    private readonly Dictionary<ushort, Opcode> opcodes = new();

    public OpcodeManager(IPluginLogger logger, OpcodeWizardConfiguration configuration, IDefinitionManager definitionManager)
    {
        this.logger = logger;
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

    private async Task Populate()
    {
        void Add(Opcode opcode, ushort code) => this.opcodes.TryAdd(code, opcode);
        void AddKeyValuePair(KeyValuePair<Opcode, ushort> keyValue) => Add(keyValue.Key, keyValue.Value);

        if (this.configuration.GameVersion != this.definitionManager.StartInfo.GameVersion?.ToString())
        {
            // reset
            this.configuration.Opcodes.Clear();
            this.configuration.RemoteUpdate = false;
            this.configuration.GameVersion = this.definitionManager.StartInfo.GameVersion?.ToString() ?? string.Empty;
            this.configuration.Save();
        }

        if (!this.configuration.RemoteUpdate)
        {
            try
            {
                using var happyEyeballsCallback = new HappyEyeballsCallback();
                using var httpClient = new HttpClient(new SocketsHttpHandler
                {
                    AutomaticDecompression = DecompressionMethods.All,
                    ConnectCallback = happyEyeballsCallback.ConnectCallback,
                });

                var updateConfigString = await httpClient.GetStringAsync("https://raw.githubusercontent.com/Athavar/Athavar.FFXIV.DalaRepo/master/opcodes.json");
                var updateConfig = JsonSerializer.Deserialize<UpdateConfig>(updateConfigString);
                if (updateConfig?.GameVersion == this.configuration.GameVersion)
                {
                    foreach (var (key, value) in updateConfig.Opcodes)
                    {
                        this.configuration.Opcodes.TryAdd(key, value);
                        Add(key, value);
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

        foreach (var (opcode, value) in this.configuration.Opcodes)
        {
            Add(opcode, value);
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

    private class UpdateConfig
    {
        [JsonPropertyName("Opcodes")]
        public Dictionary<Opcode, ushort> Opcodes { get; set; } = new();

        [JsonPropertyName("GameVersion")]
        public string GameVersion { get; set; } = string.Empty;
    }
}