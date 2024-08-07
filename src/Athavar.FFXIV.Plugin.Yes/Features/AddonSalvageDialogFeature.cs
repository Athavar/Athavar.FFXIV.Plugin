﻿// <copyright file="AddonSalvageDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;

/// <summary>
///     AddonSalvageDialog feature.
/// </summary>
internal class AddonSalvageDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonSalvageDialogFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonSalvageDialogFeature(YesModule module)
        : base(module, AddonEvent.PostSetup)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SalvageDialog";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.DesynthDialogEnabled || this.Configuration.DesynthBulkDialogEnabled;

    /// <inheritdoc/>
    protected override void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        ClickSalvageDialog salvageDialog = addon;

        if (this.Configuration.DesynthBulkDialogEnabled)
        {
            salvageDialog.CheckBox();
        }

        if (this.Configuration.DesynthDialogEnabled)
        {
            this.Module.Logger.Debug("Advanced Salvage Dialog menu");
            salvageDialog.Desynthesize();
        }
    }
}