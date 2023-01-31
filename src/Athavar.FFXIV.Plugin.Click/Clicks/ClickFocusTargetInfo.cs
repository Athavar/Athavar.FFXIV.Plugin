// <copyright file="ClickFocusTargetInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using Athavar.FFXIV.Plugin.Click.Enums;
using FFXIVClientStructs.FFXIV.Client.UI;

public class ClickFocusTargetInfo : ClickBase<ClickFocusTargetInfo, AddonTalk>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickFocusTargetInfo" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickFocusTargetInfo(nint addon = default)
        : base("_FocusTargetInfo", addon)
    {
    }

    public static implicit operator ClickFocusTargetInfo(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickFocusTargetInfo Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the talk dialog.
    /// </summary>
    [ClickName("focus_interact")]
    public void Click() => this.RightClickAddonStage(0, EventType.MOUSE_DOWN);
}