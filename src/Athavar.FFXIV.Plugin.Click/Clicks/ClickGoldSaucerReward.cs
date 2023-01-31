// <copyright file="ClickGoldSaucerReward.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;

/// <summary>
///     Addon GoldSaucerReward.
/// </summary>
public sealed class ClickGoldSaucerReward : ClickBase<ClickGoldSaucerReward>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickGoldSaucerReward" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickGoldSaucerReward(nint addon = default)
        : base("GoldSaucerReward", addon)
    {
    }

    public static implicit operator ClickGoldSaucerReward(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickGoldSaucerReward Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the quit button.
    /// </summary>
    [ClickName("gold_saucer_reward_quit")]
    public void Quit() => this.FireCallback(-1);
}