// <copyright file="ClickFocusTargetInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Attributes;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Enums;
using FFXIVClientStructs.FFXIV.Client.UI;

public class ClickFocusTargetInfo : ClickBase<ClickContextIconMenu, AddonTalk>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickFocusTargetInfo" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickFocusTargetInfo(IntPtr addon = default)
        : base("_FocusTargetInfo", addon)
    {
    }

    public static implicit operator ClickFocusTargetInfo(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickFocusTargetInfo Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the talk dialog.
    /// </summary>
    [ClickName("focus_interact")]
    public void Click() => this.RightClickAddonStage(0, EventType.MOUSE_DOWN);
}