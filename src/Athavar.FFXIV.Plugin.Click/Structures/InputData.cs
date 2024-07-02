// <copyright file="InputData.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Structures;

using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     Input data.
/// </summary>
public sealed unsafe class InputData : SharedBuffer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InputData"/> class.
    /// </summary>
    /// <returns>Input data.</returns>
    public static AtkEventData Empty()
    {
        var data = default(AtkEventData);
        return data;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputData"/> class.
    /// </summary>
    /// <param name="popupMenu">List popup menu.</param>
    /// <param name="index">Selected index.</param>
    /// <returns>Input data.</returns>
    public static AtkEventData ForPopupMenu(PopupMenu* popupMenu, ushort index)
    {
        var data = default(AtkEventData);
        data.ListItemData.ListItemRenderer = popupMenu->List->ItemRendererList[index].AtkComponentListItemRenderer;
        data.ListItemData.SelectedIndex = index;
        /* data.Data[2] = (void*)(index | (ulong)index << 48); */
        return data;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputData"/> class.
    /// </summary>
    /// <param name="node">List popup menu.</param>
    /// <param name="index">Selected index.</param>
    /// <returns>Input data.</returns>
    public static AtkEventData ForAtkComponentNode(AtkComponentNode* node, ushort index)
    {
        var data = default(AtkEventData);
        data.ListItemData.ListItemRenderer = ((AtkComponentList*)node->Component)->ItemRendererList[index].AtkComponentListItemRenderer;
        data.ListItemData.SelectedIndex = index;
        /* data.Data[2] = (void*)(index | (ulong)index << 48); */
        return data;
    }
}