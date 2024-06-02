// <copyright file="AddonSalvageResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;

internal class AddonSalvageResultFeature : OnSetupFeature
{
    public AddonSalvageResultFeature(YesModule module)
        : base(module, AddonEvent.PostUpdate)
    {
    }

    protected override string AddonName => "SalvageResult";

    protected override bool ConfigurationEnableState => this.Configuration.DesynthResultsEnabled;

    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var unitBase = (AtkUnitBase*)addon;

        this.module.Logger.Debug("Closing Salvage Results menu");
        unitBase->Close(true);
    }
}