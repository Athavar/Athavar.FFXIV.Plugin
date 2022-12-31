// <copyright file="HuntLinkDatastore.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.HuntLink;

using System;
using System.Collections.Generic;
using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Lumina.Excel.GeneratedSheets;

internal class HuntLinkDatastore : IDisposable
{
    public World? SelectedWorld;

    private readonly IDalamudServices dalamudServices;
    // private readonly List<(int, MapLinkPayload)> Fates = new();

    public HuntLinkDatastore(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;

        this.PopulateWorlds();
        this.PopulateFates();

        this.dalamudServices.ClientState.Login += this.OnLogin;
        this.dalamudServices.ClientState.Logout += this.OnLogout;
    }

    private enum LinkType
    {
        Fate,
        BRank,
        ARank,
        SRank,
    }

    public IList<World> Worlds { get; private set; } = new List<World>();

    public IList<Fate> Fates { get; private set; } = new List<Fate>();

    public void Dispose()
    {
        this.dalamudServices.ClientState.Login -= this.OnLogin;
        this.dalamudServices.ClientState.Logout -= this.OnLogout;
    }

    private void OnLogout(object? sender, EventArgs args) => this.Worlds.Clear();

    private void OnLogin(object? sender, EventArgs args) => this.PopulateWorlds();

    private void PopulateWorlds()
    {
        // current datacenter.
        var hw = this.dalamudServices.ClientState.LocalPlayer?.HomeWorld;
        var dc = this.dalamudServices.ClientState.LocalPlayer?.HomeWorld.GameData.DataCenter.Row;

        if (hw is null || dc is null)
        {
            return;
        }

        this.SelectedWorld = hw.GameData;

        this.Worlds = this.dalamudServices.DataManager.GetExcelSheet<World>()!.Where(w => w.DataCenter.Row == dc).ToList();
    }

    private void PopulateFates() => this.Fates = this.dalamudServices.DataManager.GetExcelSheet<Fate>()!.Where(w => w.Unknown24 == 1).ToList();
}