// <copyright file="ClickCharaSelectListMenu.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using Athavar.FFXIV.Plugin.Click.Enums;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

public sealed class ClickCharaSelectListMenu : ClickBase<ClickCharaSelectListMenu, AddonTalk>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickCharaSelectListMenu" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickCharaSelectListMenu(nint addon = default)
        : base("_CharaSelectListMenu", addon)
    {
    }

    public static implicit operator ClickCharaSelectListMenu(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickCharaSelectListMenu Using(nint addon) => new(addon);

    /// <summary>
    ///     Click a retainer.
    /// </summary>
    /// <param name="index">Character index.</param>
    public unsafe void Character(ushort index) => this.ClickAddonComponentList((AtkComponentNode*)this.UnitBase->UldManager.NodeList[2], index, 5U + index, EventType.MOUSE_CLICK);

#pragma warning disable SA1134,SA1516,SA1600
    [ClickName("login_character1")]
    public void Character1() => this.Character(0);

    [ClickName("login_character2")]
    public void Character2() => this.Character(1);

    [ClickName("login_character3")]
    public void Character3() => this.Character(2);

    [ClickName("login_character4")]
    public void Character4() => this.Character(3);

    [ClickName("login_character5")]
    public void Character5() => this.Character(4);

    [ClickName("login_character6")]
    public void Character6() => this.Character(5);

    [ClickName("login_character7")]
    public void Character7() => this.Character(6);

    [ClickName("login_character8")]
    public void Character8() => this.Character(7);
#pragma warning restore SA1134,SA1516,SA1600
}