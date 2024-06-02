// <copyright file="ClickPurifyResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Bases;

public class ClickPurifyResult : ClickBase<ClickRecipeNote>
{
    public ClickPurifyResult(IntPtr addon)
        : base("PurifyResult", addon)
    {
    }

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickMaterialize Using(nint addon) => new(addon);
}