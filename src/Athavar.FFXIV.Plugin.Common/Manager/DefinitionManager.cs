// <copyright file="DefinitionManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Athavar.FFXIV.Plugin.Common.Definitions;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Common;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Action = Athavar.FFXIV.Plugin.Common.Definitions.Action;

internal sealed class DefinitionManager : IDefinitionManager
{
    private static readonly string DefinitionFolderName = "Athavar.FFXIV.Plugin.Common.Definitions.Data";
    private static readonly string JobDefinitionFolderName = DefinitionFolderName + ".Jobs";

    private readonly IDalamudServices services;
    private readonly IDataManager dataManager;

    private Dictionary<uint, Action> actionEffectDefinitions;
    private Dictionary<uint, StatusEffect> statusEffectDefinitions;
    private TerritoryContentNames territoryContentNames;

    public DefinitionManager(IDalamudServices services)
    {
        this.services = services;
        this.dataManager = services.DataManager;
        this.LoadResources();

        if (TryGetDalamudStartInfo(services, out var dalamudStartInfo))
        {
            this.StartInfo = dalamudStartInfo;
        }
    }

    /// <inheritdoc/>
    public DalamudStartInfo StartInfo { get; } = new();

    /// <inheritdoc/>
    public Action? GetActionById(uint actionId) => this.actionEffectDefinitions.GetValueOrDefault(actionId);

    /// <inheritdoc/>
    public StatusEffect? GetStatusEffectById(uint statusId) => this.statusEffectDefinitions.GetValueOrDefault(statusId);

    /// <inheritdoc/>
    public uint[] GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType procType) => this.statusEffectDefinitions.Where(x => x.Value.ReactiveProc?.Type == procType).Select(x => x.Key).ToArray();

    /// <inheritdoc/>
    public MultiString GetContentName(uint territoryId)
    {
        if (!this.territoryContentNames.TryGetValue(territoryId, out var name))
        {
            var contentFinderId = this.dataManager.GetExcelSheet<TerritoryType>().GetRow(territoryId).ContentFinderCondition.RowId;
            name = contentFinderId == 0 ? MultiString.Empty : MultiStringUtils.FromContentFinderCondition(this.dataManager, contentFinderId);
            this.territoryContentNames.TryAdd(territoryId, name);
        }

        return name;
    }

    private static bool TryGetDalamudStartInfo(IDalamudServices services, [NotNullWhen(true)] out DalamudStartInfo? dalamudStartInfo)
    {
        dalamudStartInfo = null;
        try
        {
            var dalamudServiceType = typeof(IDalamudPluginInterface).Assembly.GetType("Dalamud.Dalamud") ?? throw new Exception("Fail to get type of Dalamud.Dalamud");
            var dalamudService = services.GetInternalService(dalamudServiceType);

            var startInfoProperty = dalamudServiceType.GetProperty("StartInfo", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new Exception("Dalamud has changed. StartInfo not found.");
            var startInfo = startInfoProperty.GetMethod!.Invoke(dalamudService, BindingFlags.NonPublic | BindingFlags.Instance, null, [], null);

            if (startInfo is DalamudStartInfo dInfo)
            {
                dalamudStartInfo = dInfo;
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            services.PluginLogger.Error($"{e.Message}\n{e.StackTrace ?? string.Empty}");
            return false;
        }
    }

    private string[] GetAllJobDefinitionFileNames()
        => typeof(DefinitionManager).Module.Assembly.GetManifestResourceNames()
           .Where(x => x.StartsWith(JobDefinitionFolderName, StringComparison.OrdinalIgnoreCase))
           .Select(x => x[DefinitionFolderName.Length..]).ToArray();

    [MemberNotNull(nameof(actionEffectDefinitions))]
    [MemberNotNull(nameof(statusEffectDefinitions))]
    [MemberNotNull(nameof(territoryContentNames))]
    private void LoadResources()
    {
        this.actionEffectDefinitions = new Dictionary<uint, Action>();
        this.statusEffectDefinitions = new Dictionary<uint, StatusEffect>();
        foreach (var definitionFileName in this.GetAllJobDefinitionFileNames())
        {
            var jobDefinition = this.ReadDefinition<JobDefinition>(definitionFileName);
            foreach (var action in jobDefinition.Actions.Values)
            {
                if (!this.actionEffectDefinitions.TryAdd(action.Id, action))
                {
                    throw new ResourceParseException($"Action Id {action.Id:X2} found for Job {jobDefinition.Job} but already added.");
                }
            }

            foreach (var statusEffect in jobDefinition.Statuseffects.Values)
            {
                if (!this.statusEffectDefinitions.TryAdd(statusEffect.Id, statusEffect))
                {
                    throw new ResourceParseException($"Status Id {statusEffect.Id:X2} found for Job {jobDefinition.Job} but already added.");
                }
            }
        }

        this.PopulateLimitToActionIds();

        this.territoryContentNames = this.ReadDefinition<TerritoryContentNames>(".TerritoryContentNames.json");
    }

    private void PopulateLimitToActionIds()
    {
        foreach (var potency in this.statusEffectDefinitions.Values.SelectMany(x => x.PotencyEffects ?? []))
        {
            if (!string.IsNullOrWhiteSpace(potency.LimitTo))
            {
                var uintList = new List<uint>();
                var limitTo = potency.LimitTo;
                foreach (var str in limitTo.Split(','))
                {
                    var abilityName = str;
                    var array = this.actionEffectDefinitions.Where(x => x.Value.Name.Equals(abilityName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Key).ToArray();
                    uintList.AddRange(array);
                }

                potency.LimitToActionIds = uintList.ToArray();
            }
            else
            {
                potency.LimitToActionIds = [];
            }
        }

        foreach (var multiplier in this.statusEffectDefinitions.Values.SelectMany(x => x.Multipliers ?? []))
        {
            if (!string.IsNullOrWhiteSpace(multiplier.LimitTo))
            {
                var uintList = new List<uint>();
                var limitTo = multiplier.LimitTo;
                foreach (var str in limitTo.Split(','))
                {
                    var abilityName = str;
                    var array = this.actionEffectDefinitions.Where(x => x.Value.Name.Equals(abilityName, StringComparison.OrdinalIgnoreCase)).Select(x => x.Key).ToArray();
                    uintList.AddRange(array);
                }

                multiplier.LimitToActionIds = uintList.ToArray();
            }
            else
            {
                multiplier.LimitToActionIds = [];
            }
        }
    }

    private T ReadDefinition<T>(string fileName)
    {
        this.services.PluginLogger.Information(fileName);
        var content = new ResourceFile(fileName).Content;
        try
        {
            return JsonSerializer.Deserialize<T>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                }) ?? throw new JsonParseException($"Error while parsing definition file {fileName}");
        }
        catch (Exception ex)
        {
            throw new ResourceParseException($"Error while parsing definition file {fileName}: {ex}");
        }
    }

    private class ResourceFile
    {
        private readonly string fileName;

        public ResourceFile(string fileName) => this.fileName = fileName;

        public string Content => this.ReadFileAsString();

        private string ReadFileAsString()
        {
            using var manifestResourceStream = typeof(ResourceFile).Module.Assembly.GetManifestResourceStream(DefinitionFolderName + this.fileName);
            if (manifestResourceStream is null)
            {
                return string.Empty;
            }

            using var streamReader = new StreamReader(manifestResourceStream);
            return streamReader.ReadToEnd();
        }
    }
}