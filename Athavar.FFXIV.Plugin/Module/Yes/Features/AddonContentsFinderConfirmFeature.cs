// <copyright file="AddonContentsFinderConfirmFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

/// <summary>
///     AddonContentsFinderConfirm feature.
/// </summary>
internal class AddonContentsFinderConfirmFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonContentsFinderConfirmFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonContentsFinderConfirmFeature(YesModule module)
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 30 44 8B F2 49 8B E8 BA ?? ?? ?? ?? 48 8B D9", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "ContentsFinderConfirm";

    /// <inheritdoc />
    protected override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.ContentsFinderConfirmEnabled)
        {
            return;
        }

        ClickContentsFinderConfirm.Using(addon).Commence();

        if (this.Configuration.ContentsFinderOneTimeConfirmEnabled)
        {
            this.Configuration.ContentsFinderConfirmEnabled = false;
            this.Configuration.ContentsFinderOneTimeConfirmEnabled = false;
            this.Configuration.Save();
        }
    }
}