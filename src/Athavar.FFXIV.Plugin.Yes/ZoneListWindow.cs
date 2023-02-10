// <copyright file="ZoneListWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;

/// <summary>
///     Zone list window.
/// </summary>
internal class ZoneListWindow : Window
{
    private readonly IDalamudServices dalamudServices;
    private YesModule? module;
    private bool sortZoneByName;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ZoneListWindow" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="windowSystem"><see cref="WindowSystem" /> added by DI.</param>
    public ZoneListWindow(IDalamudServices dalamudServices, WindowSystem windowSystem)
        : base("Zone List")
    {
        this.dalamudServices = dalamudServices;
        this.Size = new Vector2(525, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;

        windowSystem.AddWindow(this);
    }

    /// <inheritdoc />
    public override void PreDraw() => ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);

    /// <inheritdoc />
    public override void PostDraw() => ImGui.PopStyleColor();

    /// <inheritdoc />
    public override void Draw()
    {
        ImGui.Text($"Current ID: {this.dalamudServices.ClientState.TerritoryType}");

        ImGui.Checkbox("Sort by Name", ref this.sortZoneByName);

        ImGui.Columns(2);

        ImGui.Text("ID");
        ImGui.NextColumn();

        ImGui.Text("Name");
        ImGui.NextColumn();

        ImGui.Separator();

        var names = this.module?.TerritoryNames.AsEnumerable();

        if (names is null)
        {
            return;
        }

        if (this.sortZoneByName)
        {
            names = names.ToList().OrderBy(kvp => kvp.Value);
        }

        foreach (var kvp in names)
        {
            ImGui.Text($"{kvp.Key}");
            ImGui.NextColumn();

            ImGui.Text($"{kvp.Value}");
            ImGui.NextColumn();
        }

        ImGui.Columns(1);
    }

    /// <summary>
    ///     Setup the <see cref="ZoneListWindow" />.
    /// </summary>
    /// <param name="m">The <see cref="YesModule" />.</param>
    internal void Setup(YesModule m) => this.module = m;
}