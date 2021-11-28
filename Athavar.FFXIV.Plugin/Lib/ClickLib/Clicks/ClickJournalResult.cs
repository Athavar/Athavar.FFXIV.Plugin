// <copyright file="ClickJournalResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon JournalResult.
/// </summary>
public sealed unsafe class ClickJournalResult : ClickAddonBase<AddonJournalResult>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickJournalResult" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickJournalResult(IntPtr addon = default)
        : base(addon)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "JournalResult";

    public static implicit operator ClickJournalResult(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickJournalResult Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the complete button.
    /// </summary>
    [ClickName("journal_result_complete")]
    public void Complete() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->CompleteButton, 1);

    /// <summary>
    ///     Click the decline button.
    /// </summary>
    [ClickName("journal_result_decline")]
    public void Decline() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->DeclineButton, 2);
}