// <copyright file="ImporterTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer;

using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Importer.Tab;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

public sealed class ImporterTab : Common.UI.Tab
{
    private readonly TabBarHandler tabBarHandler;
    private readonly IDalamudServices dalamudServices;

    public ImporterTab(IDalamudServices dalamudServices, IIconManager iconManager, IIpcManager ipcManager)
    {
        this.dalamudServices = dalamudServices;
        this.tabBarHandler = new TabBarHandler(this.dalamudServices.PluginLogger, "importers");
        this.tabBarHandler.Add(new EorzeaCollectionTab(this.dalamudServices, iconManager, ipcManager));
    }

    public override string Name => "Importer";

    public override string Identifier => "importer";

    public override void Draw() => this.tabBarHandler.Draw();
}