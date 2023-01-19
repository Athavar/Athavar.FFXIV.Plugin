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

internal class CraftQueueTab
{
    private readonly TabBarHandler tabBarHandler = new("CraftQueueTabBar");

    private string debugInput = string.Empty;

    public CraftQueueTab(IDalamudServices dalamudServices, IIconCacheManager iconCacheManager, IChatManager chatManager, Configuration configuration, IGearsetManager gearsetManager)
    {
        this.BaseConfiguration = configuration;

        var dataManager = dalamudServices.DataManager;

        this.ClientLanguage = dalamudServices.ClientState.ClientLanguage;

        this.tabBarHandler
           .Register(new StatsTab(gearsetManager, dataManager))
           .Register(new RotationTab(this.Configuration, chatManager, iconCacheManager, this.ClientLanguage))
           .Register(new QueueTab(dataManager, iconCacheManager, gearsetManager, this.Configuration, this.ClientLanguage))
           .Register(new DebugTab(gearsetManager));
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

        public DebugTab(IGearsetManager gearsetManager) => this.gearsetManager = gearsetManager;

        protected internal override string Name => "Debug";

        protected internal override string Identifier => "Tab-CQDebug";

        public override void Draw()
        {
            if (ImGui.Button("Update"))
            {
                this.gearsetManager.UpdateGearsets();
            }
        }
    }
}