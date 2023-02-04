// <copyright file="DefinitionManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Athavar.FFXIV.Plugin.Common.Definitions;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud;
using Dalamud.Logging;
using Dalamud.Plugin;

public class DefinitionManager : IDefinitionManager
{
    private static readonly string DefinitionFolderName = "Athavar.FFXIV.Plugin.Common.Definitions.Data";

    private Dictionary<uint, Action> actionEffectDefinitions;
    private Dictionary<uint, StatusEffect> statusEffectDefinitions;

    public DefinitionManager(IDalamudServices services)
    {
        this.LoadResources();

        if (TryGetDalamudStartInfo(services.PluginInterface, out var dalamudStartInfo))
        {
            this.StartInfo = dalamudStartInfo;
        }
    }

    /// <inheritdoc />
    public DalamudStartInfo StartInfo { get; } = new();

    /// <inheritdoc />
    public Action? GetActionById(uint actionId) => this.actionEffectDefinitions.ContainsKey(actionId) ? this.actionEffectDefinitions[actionId] : null;

    /// <inheritdoc />
    public StatusEffect? GetStatusEffectById(uint statusId) => this.statusEffectDefinitions.ContainsKey(statusId) ? this.statusEffectDefinitions[statusId] : null;

    /// <inheritdoc />
    public uint[] GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType procType) => this.statusEffectDefinitions.Where(x => x.Value.ReactiveProc?.Type == procType).Select(x => x.Key).ToArray();

    private static bool TryGetDalamudStartInfo(DalamudPluginInterface pluginInterface, [NotNullWhen(true)] out DalamudStartInfo? dalamudStartInfo)
    {
        dalamudStartInfo = null;
        try
        {
            var info = pluginInterface.GetType().Assembly.GetType("Dalamud.Service`1", true)?.MakeGenericType(typeof(DalamudStartInfo)).GetMethod("Get")?.Invoke(null, BindingFlags.Default, null, Array.Empty<object>(), null);

            if (info is DalamudStartInfo dInfo)
            {
                dalamudStartInfo = dInfo;
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            PluginLog.Error($"{e.Message}\n{e.StackTrace ?? string.Empty}");
            return false;
        }
    }

    private string[] GetAllJobDefinitionFileNames()
        => typeof(DefinitionManager).Module.Assembly.GetManifestResourceNames()
           .Where(x => x.StartsWith(DefinitionFolderName, StringComparison.OrdinalIgnoreCase))
           .Select(x => x[DefinitionFolderName.Length..]).ToArray();

    [MemberNotNull(nameof(actionEffectDefinitions))]
    [MemberNotNull(nameof(statusEffectDefinitions))]
    private void LoadResources()
    {
        this.actionEffectDefinitions = new Dictionary<uint, Action>();
        this.statusEffectDefinitions = new Dictionary<uint, StatusEffect>();
        foreach (var definitionFileName in this.GetAllJobDefinitionFileNames())
        {
            var jobDefinition = this.ReadJobDefinition(definitionFileName);
            foreach (var action in jobDefinition.Actions.Values)
            {
                if (this.actionEffectDefinitions.ContainsKey(action.Id))
                {
                    throw new ResourceParseException($"Action Id {action.Id:X2} found for Job {jobDefinition.Job} but already added.");
                }

                this.actionEffectDefinitions.Add(action.Id, action);
            }

            foreach (var statusEffect in jobDefinition.Statuseffects.Values)
            {
                if (this.statusEffectDefinitions.ContainsKey(statusEffect.Id))
                {
                    throw new ResourceParseException($"Status Id {statusEffect.Id:X2} found for Job {jobDefinition.Job} but already added.");
                }

                this.statusEffectDefinitions.Add(statusEffect.Id, statusEffect);
            }
        }

        this.PopulateLimitToActionIds();
    }

    private void PopulateLimitToActionIds()
    {
        foreach (var potency in this.statusEffectDefinitions.Values.SelectMany(x => x.PotencyEffects ?? Array.Empty<Potency>()))
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
                potency.LimitToActionIds = Array.Empty<uint>();
            }
        }

        foreach (var multiplier in this.statusEffectDefinitions.Values.SelectMany(x => x.Multipliers ?? Array.Empty<Multiplier>()))
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
                multiplier.LimitToActionIds = Array.Empty<uint>();
            }
        }
    }

    private JobDefinition ReadJobDefinition(string fileName)
    {
        var content = new ResourceFile(fileName).Content;
        try
        {
            return JsonSerializer.Deserialize<JobDefinition>(content, new JsonSerializerOptions
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