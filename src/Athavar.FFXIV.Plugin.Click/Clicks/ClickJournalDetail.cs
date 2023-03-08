// <copyright file="ClickJournalDetail.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon JournalDetail.
/// </summary>
public sealed unsafe class ClickJournalDetail : ClickBase<ClickJournalDetail, AddonJournalDetail>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickJournalDetail" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickJournalDetail(nint addon = default)
        : base("JournalDetail", addon)
    {
    }

    public static implicit operator ClickJournalDetail(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickJournalDetail Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the accept button.
    /// </summary>
    [ClickName("journal_detail_accept")]
    public void Accept() => this.ClickAddonButton(this.Addon->AcceptMapButton, 1);

    /// <summary>
    ///     Click the decline button.
    /// </summary>
    [ClickName("journal_detail_decline")]
    public void Decline() => this.ClickAddonButton(this.Addon->InitiateButton, 2);
}