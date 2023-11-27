// <copyright file="ColumnBool.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Types;

using Athavar.FFXIV.Plugin.Common.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility.Table;
using ImGuiNET;

public class ColumnBool<TItem> : Column<TItem>
{
    private bool? filterValue;

    public string? Tooltip { get; set; }

    public virtual bool ToBool(TItem item) => true;

    public override bool DrawFilter()
    {
        using var id = ImRaii.PushId(this.FilterLabel);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var all = this.filterValue is null;
        using var color = ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
        using var combo = ImRaii.Combo(string.Empty, this.Label, ImGuiComboFlags.NoArrowButton);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            this.filterValue = null;
            return true;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(this.Tooltip ?? this.Label);
        }

        if (!all && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Right-click to clear filters.");
        }

        if (!combo)
        {
            return false;
        }

        color.Pop();

        using var font = ImRaii.PushFont(UiBuilder.IconFont);

        var ret = false;
        var stateY = all | this.filterValue ?? true;
        using (ImRaii.PushColor(ImGuiCol.Text, 0xFF008000))
        {
            if (ImGui.Checkbox(FontAwesomeIcon.Check.ToIconString(), ref stateY))
            {
                ret = true;
            }
        }

        var stateX = all | !this.filterValue ?? true;
        using (ImRaii.PushColor(ImGuiCol.Text, 0xFF000080))
        {
            if (ImGui.Checkbox(FontAwesomeIcon.Times.ToIconString(), ref stateX))
            {
                ret = true;
            }
        }

        if (ret)
        {
            this.filterValue = stateY && stateX ? null : stateY ? true : stateX ? false : !this.filterValue;
        }

        return ret;
    }

    public override bool FilterFunc(TItem item)
    {
        if (this.filterValue is null)
        {
            return true;
        }

        var boolean = this.ToBool(item);

        return this.filterValue == boolean;
    }

    public override void DrawColumn(TItem item, int idx)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        if (this.ToBool(item))
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, 0xFF008000);
            ImGuiEx.Center(FontAwesomeIcon.Check.ToIconString());
        }
        else
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, 0xFF000080);
            ImGuiEx.Center(FontAwesomeIcon.Times.ToIconString());
        }
    }

    public override int Compare(TItem lhs, TItem rhs) => this.ToBool(lhs).CompareTo(this.ToBool(rhs));
}