// <copyright file="ClickGrandCompanySupplyList.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

internal class ClickGrandCompanySupplyList : ClickBase<ClickGrandCompanySupplyList, AddonTalk>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickGrandCompanySupplyList"/> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickGrandCompanySupplyList(nint addon = default)
        : base("GrandCompanySupplyList", addon)
    {
    }

    public static implicit operator ClickGrandCompanySupplyList(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickGrandCompanySupplyList Using(nint addon) => new(addon);

    /// <summary>
    ///     Click a Supply.
    /// </summary>
    /// <param name="index">Supply index.</param>
    public unsafe void Supply(ushort index) => this.ClickAddonComponentList((AtkComponentNode*)this.UnitBase->UldManager.NodeList[5], index, 1U + index);

    [ClickName("select_gcsupply1")]
    public void Supply1() => this.Supply(0);
}