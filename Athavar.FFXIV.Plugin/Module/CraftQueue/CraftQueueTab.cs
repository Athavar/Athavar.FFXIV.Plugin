// <copyright file="CraftQueueTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using System;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.UI;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

internal class CraftQueueTab
{
    private readonly TabBarHandler tabBarHandler = new("CraftQueueTabBar");

    private string debugInput = string.Empty;

    public CraftQueueTab(IServiceProvider serviceProvider, Configuration configuration)
    {
        this.BaseConfiguration = configuration;

        var dalamudServices = serviceProvider.GetRequiredService<IDalamudServices>();
        var dataManager = dalamudServices.DataManager;

        var commandInterface = serviceProvider.GetRequiredService<ICommandInterface>();
        var iconCacheManager = serviceProvider.GetRequiredService<IIconCacheManager>();
        var chatManager = serviceProvider.GetRequiredService<IChatManager>();
        var gearsetManager = serviceProvider.GetRequiredService<IGearsetManager>();

        var craftQueue = serviceProvider.GetRequiredService<CraftQueue>();

        this.ClientLanguage = dalamudServices.ClientState.ClientLanguage;

        this.tabBarHandler
           .Register(new StatsTab(gearsetManager, dataManager))
           .Register(new RotationTab(this.Configuration, chatManager, iconCacheManager, this.ClientLanguage))
           .Register(new QueueTab(iconCacheManager, craftQueue, this.Configuration, this.ClientLanguage))
#if DEBUG
           .Register(new DebugTab(gearsetManager, commandInterface, craftQueue))
#endif
            ;
    }

    private ClientLanguage ClientLanguage { get; }

    private Configuration BaseConfiguration { get; }

    private CraftQueueConfiguration Configuration => this.BaseConfiguration.CraftQueue ?? throw new ArgumentException();

    /// <summary>
    ///     Draw this tab.
    /// </summary>
    internal void DrawTab()
    {
        using var raii = new ImGuiRaii();
        if (!raii.Begin(() => ImGui.BeginTabItem("CraftQueue"), ImGui.EndTabItem))
        {
            return;
        }

        this.tabBarHandler.Draw();
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

        protected internal override string Name => "Debug";

        protected internal override string Identifier => "Tab-CQDebug";

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