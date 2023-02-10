// <copyright file="ClickGrandCompanySupplyReward.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon GrandCompanySupplyReward.
/// </summary>
public sealed unsafe class ClickGrandCompanySupplyReward : ClickBase<ClickGrandCompanySupplyReward, AddonGrandCompanySupplyReward>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickGrandCompanySupplyReward" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickGrandCompanySupplyReward(nint addon = default)
        : base("GrandCompanySupplyReward", addon)
    {
    }

    public static implicit operator ClickGrandCompanySupplyReward(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickGrandCompanySupplyReward Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the deliver button.
    /// </summary>
    [ClickName("grand_company_expert_delivery_deliver")]
    public void Deliver() => this.ClickAddonButton(this.Addon->DeliverButton, 0);

    /// <summary>
    ///     Click the cancel button.
    /// </summary>
    [ClickName("grand_company_expert_delivery_cancel")]
    public void Cancel() => this.ClickAddonButton(this.Addon->CancelButton, 1);
}