// <copyright file="CraftQueueTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.CraftQueue.Job;
using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.CraftQueue.UI;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game;
using Dalamud.Plugin;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

internal sealed class CraftQueueTab : Tab
{
    private const string CraftimizerAssemblyName = "Craftimizer";

    private readonly IServiceProvider serviceProvider;
    private readonly IPluginMonitorService monitorService;
    private readonly IDalamudServices dalamudServices;
    private readonly TabBarHandler tabBarHandler;
    private readonly List<IRotationResolver> rotationResolvers;

    private bool featureEnabledCraftimizer;

    public CraftQueueTab(IServiceProvider serviceProvider, IPluginMonitorService monitorService, CraftQueue craftQueue, CraftQueueConfiguration configuration)
    {
        this.Configuration = configuration;

        this.serviceProvider = serviceProvider;
        this.monitorService = monitorService;
        this.dalamudServices = serviceProvider.GetRequiredService<IDalamudServices>();

        var commandInterface = serviceProvider.GetRequiredService<ICommandInterface>();
        var iconCacheManager = serviceProvider.GetRequiredService<IIconManager>();
        var chatManager = serviceProvider.GetRequiredService<IChatManager>();
        var gearsetManager = serviceProvider.GetRequiredService<IGearsetManager>();
        var craftSkillManager = serviceProvider.GetRequiredService<ICraftDataManager>();
        this.ClientLanguage = this.dalamudServices.ClientState.ClientLanguage;

        var queueTab = new QueueTab(craftQueue, iconCacheManager, craftSkillManager, this.Configuration, this.ClientLanguage);
        this.rotationResolvers = queueTab.ExternalResolver;

        this.tabBarHandler = new TabBarHandler(this.dalamudServices.PluginLogger, "CraftQueueTabBar");
        this.tabBarHandler
           .Add(new StatsTab(craftQueue))
           .Add(new RotationTab(craftQueue, this.Configuration, chatManager, iconCacheManager, craftSkillManager, this.ClientLanguage))
           .Add(queueTab)
           .Add(new ConfigTab(this.Configuration))
#if DEBUG
           .Add(new DebugTab(gearsetManager, commandInterface, craftQueue))
#endif
            ;

        this.UpdateCraftimizerIntegration(monitorService.IsLoaded(CraftimizerAssemblyName));
        this.monitorService.LoadingStateHasChanged += this.OnPluginLoadingStateHasChanged;
    }

    /// <inheritdoc/>
    public override string Name => "CraftQueue";

    /// <inheritdoc/>
    public override string Identifier => "cq-tab";

    /// <inheritdoc/>
    public override string Title => $"{this.Name} > {this.tabBarHandler.GetTabTitle()}";

    private ClientLanguage ClientLanguage { get; }

    private CraftQueueConfiguration Configuration { get; }

    /// <summary>
    ///     Draw this tab.
    /// </summary>
    public override void Draw() => this.tabBarHandler.Draw();

    public override void Dispose()
    {
        this.monitorService.LoadingStateHasChanged -= this.OnPluginLoadingStateHasChanged;
        base.Dispose();
    }

    private void OnPluginLoadingStateHasChanged(string name, bool state, IExposedPlugin? plugin)
    {
        if (name == CraftimizerAssemblyName)
        {
            this.UpdateCraftimizerIntegration(state);
        }
    }

    private void UpdateCraftimizerIntegration(bool enableState)
    {
        if (enableState)
        {
            this.rotationResolvers.Add(ActivatorUtilities.CreateInstance<CraftimizerResolver>(this.serviceProvider));
        }
        else
        {
            this.rotationResolvers.RemoveAll(o => o.GetType() == typeof(CraftimizerResolver));
        }

        this.featureEnabledCraftimizer = enableState;
    }

    private class DebugTab : Tab
    {
        private readonly IGearsetManager gearsetManager;
        private readonly ICommandInterface commandInterface;
        private readonly CraftQueue craftQueue;

        private Gearset? current;

        public DebugTab(IGearsetManager gearsetManager, ICommandInterface commandInterface, CraftQueue craftQueue)
        {
            this.gearsetManager = gearsetManager;
            this.commandInterface = commandInterface;
            this.craftQueue = craftQueue;
        }

        public override string Name => "Debug";

        public override string Identifier => "Tab-CQDebug";

        public override void Draw()
        {
            if (ImGui.Button("Update"))
            {
                this.gearsetManager.UpdateGearsets();
            }

            ImGui.TextUnformatted($"Index: {this.commandInterface.GetRecipeNoteSelectedRecipeId()}");
            ImGui.TextUnformatted($"Food: {this.craftQueue.Data.Foods.Count}");
            ImGui.TextUnformatted($"Potion: {this.craftQueue.Data.Potions.Count}");

            if (ImGui.Button("Update Gear"))
            {
                this.current = this.gearsetManager.GetCurrentEquipment() ?? throw new CraftingJobException("Fail to get the current gear-stats.");
            }

            if (this.current is not null)
            {
                ImGui.TextUnformatted($"Control: {this.current.Control}");
                ImGui.TextUnformatted($"Craftsmanship: {this.current.Craftsmanship}");
                ImGui.TextUnformatted($"CP: {this.current.CP}");
            }
        }
    }
}