// <copyright file="ClickJournalResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Attributes;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon JournalResult.
/// </summary>
public sealed unsafe class ClickJournalResult : ClickBase<ClickJournalResult, AddonJournalResult>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickJournalResult" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickJournalResult(IntPtr addon = default)
        : base("JournalResult", addon)
    {
    }

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
    public void Complete() => this.ClickAddonButton(this.Addon->CompleteButton, 1);

    /// <summary>
    ///     Click the decline button.
    /// </summary>
    [ClickName("journal_result_decline")]
    public void Decline() => this.ClickAddonButton(this.Addon->DeclineButton, 2);
}