// <copyright file="CraftQueueTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.CraftQueue.UI;
using Dalamud;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

internal class CraftQueueTab : Tab
{
    private readonly TabBarHandler tabBarHandler = new("CraftQueueTabBar");

    private string debugInput = string.Empty;

    public CraftQueueTab(IServiceProvider serviceProvider, CraftQueue craftQueue, Configuration configuration)
    {
        this.BaseConfiguration = configuration;

        var dalamudServices = serviceProvider.GetRequiredService<IDalamudServices>();
        var dataManager = dalamudServices.DataManager;

        var commandInterface = serviceProvider.GetRequiredService<ICommandInterface>();
        var iconCacheManager = serviceProvider.GetRequiredService<IIconManager>();
        var chatManager = serviceProvider.GetRequiredService<IChatManager>();
        var gearsetManager = serviceProvider.GetRequiredService<IGearsetManager>();
        this.ClientLanguage = dalamudServices.ClientState.ClientLanguage;

        this.tabBarHandler
           .Add(new StatsTab(gearsetManager, dataManager))
           .Add(new RotationTab(this.Configuration, chatManager, iconCacheManager, this.ClientLanguage))
           .Add(new QueueTab(iconCacheManager, craftQueue, this.Configuration, this.ClientLanguage))
           .Add(new ConfigTab(this.Configuration))
#if DEBUG
           .Add(new DebugTab(gearsetManager, commandInterface, craftQueue))
#endif
            ;
    }

    /// <inheritdoc />
    public override string Name => "CraftQueue";

    /// <inheritdoc />
    public override string Identifier => "cq-tab";

    /// <inheritdoc />
    public override string Title => $"{this.Name} > {this.tabBarHandler.GetTabTitle()}";

    private ClientLanguage ClientLanguage { get; }

    private Configuration BaseConfiguration { get; }

    private CraftQueueConfiguration Configuration => this.BaseConfiguration.CraftQueue ?? throw new ArgumentException();

    /// <summary>
    ///     Draw this tab.
    /// </summary>
    public override void Draw() => this.tabBarHandler.Draw();

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