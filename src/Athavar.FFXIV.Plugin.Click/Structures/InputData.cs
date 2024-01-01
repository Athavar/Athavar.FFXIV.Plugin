// <copyright file="InputData.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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
    private InputData()
    {
        const int size = 0x40;
        this.Data = (void**)Buffer.Add(new byte[size]);
        /*
        var d = stackalloc byte[size];
        this.Data = (void**)Buffer.Add(d, size);
        */
        if (this.Data == null)
        {
            throw new ArgumentNullException(null, "InputData could not be created, null");
        }

        this.Data[0] = null;
        this.Data[1] = null;
        this.Data[2] = null;
        this.Data[3] = null;
        this.Data[4] = null;
        this.Data[5] = null;
        this.Data[6] = null;
        this.Data[7] = null;
    }

    /// <summary>
    ///     Gets the data pointer.
    /// </summary>
    public void** Data { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputData"/> class.
    /// </summary>
    /// <returns>Input data.</returns>
    public static InputData Empty() => new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputData"/> class.
    /// </summary>
    /// <param name="popupMenu">List popup menu.</param>
    /// <param name="index">Selected index.</param>
    /// <returns>Input data.</returns>
    public static InputData ForPopupMenu(PopupMenu* popupMenu, ushort index)
    {
        var data = new InputData();
        data.Data[0] = popupMenu->List->ItemRendererList[index].AtkComponentListItemRenderer;
        data.Data[2] = (void*)(index | ((ulong)index << 48));
        return data;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputData"/> class.
    /// </summary>
    /// <param name="node">List popup menu.</param>
    /// <param name="index">Selected index.</param>
    /// <returns>Input data.</returns>
    public static InputData ForAtkComponentNode(AtkComponentNode* node, ushort index)
    {
        var data = new InputData();
        data.Data[0] = ((AtkComponentList*)node->Component)->ItemRendererList[index].AtkComponentListItemRenderer;
        data.Data[2] = (void*)(index | ((ulong)index << 48));
        return data;
    }
}