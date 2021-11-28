﻿// <copyright file="ClickRecipeNote.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon RecipeNote.
/// </summary>
public sealed unsafe class ClickRecipeNote : ClickAddonBase<AddonRecipeNote>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickRecipeNote" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickRecipeNote(IntPtr addon = default)
        : base(addon)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "RecipeNote";

    public static implicit operator ClickRecipeNote(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRecipeNote Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the synthesize button.
    /// </summary>
    [ClickName("synthesize")]
    public void Synthesize() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->SynthesizeButton, 13);

    /// <summary>
    ///     Click the trial synthesis button.
    /// </summary>
    [ClickName("trial_synthesis")]
    public void TrialSynthesis() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->TrialSynthesisButton, 15);
}