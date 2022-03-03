namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon RecipeNote.
/// </summary>
public sealed unsafe class ClickSynthesis : ClickAddonBase<AddonSynthesis>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickSynthesis" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickSynthesis(IntPtr addon = default)
        : base(addon)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "Synthesis";

    public static implicit operator ClickSynthesis(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRecipeNote Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the synthesize button.
    /// </summary>
    [ClickName("synthesis_quit")]
    public void Quit() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->QuitButton, 0);
}