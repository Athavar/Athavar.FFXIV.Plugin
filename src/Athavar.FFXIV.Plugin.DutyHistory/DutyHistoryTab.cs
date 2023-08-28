// <copyright file="DutyHistoryTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.DutyHistory;

using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Models.Data;
using Dalamud.Interface.Utility.Table;
using ImGuiNET;

public sealed class DutyHistoryTab : Tab
{
    private const string TabIdentifier = "dutyhistory";
    private readonly Table<ContentEncounter> table;

    public DutyHistoryTab(DutyHistoryTable table) => this.table = table;

    public override string Name => DutyHistoryModule.ModuleName;

    public override string Identifier => TabIdentifier;

    public override void Draw()
    {
        ImGui.Separator();
        this.table.Draw(ImGui.GetTextLineHeightWithSpacing());
    }
}