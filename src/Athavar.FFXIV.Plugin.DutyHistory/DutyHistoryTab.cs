// <copyright file="DutyHistoryTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.DutyHistory;

using Athavar.FFXIV.Plugin.Common.UI;
using Dalamud.Plugin.Services;
using ImGuiNET;

public sealed class DutyHistoryTab : Tab
{
    private const string TabIdentifier = "dutyhistory";
    private readonly DutyHistoryTable table;
    private readonly IClientState clientState;

    public DutyHistoryTab(DutyHistoryTable table, IClientState clientState)
    {
        this.table = table;
        this.clientState = clientState;

        if (this.clientState.IsLoggedIn)
        {
            table.Update(this.clientState.LocalContentId);
        }

        this.clientState.Login += this.OnLogin;
    }

    /// <inheritdoc/>
    public override string Name => DutyHistoryModule.ModuleName;

    /// <inheritdoc/>
    public override string Identifier => TabIdentifier;

    /// <inheritdoc/>
    public override void Dispose()
    {
        this.clientState.Login -= this.OnLogin;
        base.Dispose();
    }

    /// <inheritdoc/>
    public override void Draw()
    {
        if (!this.clientState.IsLoggedIn)
        {
            ImGui.TextUnformatted("Please login.");
            return;
        }

        this.table.Draw(ImGui.GetTextLineHeightWithSpacing());
    }

    private void OnLogin() => this.table.Update(this.clientState.LocalContentId);
}