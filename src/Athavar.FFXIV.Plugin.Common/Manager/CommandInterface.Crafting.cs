// <copyright file="CommandInterface.Crafting.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

internal sealed partial class CommandInterface
{
    // https://github.com/ffxiv-teamcraft/simulator/blob/0682dfa76043ff4ccb38832c184d046ceaff0733/src/model/tables.ts#L2
    private static ReadOnlySpan<int> HqPercentTable
        =>
        [
            1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8,
            9, 9, 9, 10, 10, 10, 11, 11, 11, 12, 12, 12, 13, 13, 13, 14, 14, 14, 15, 15, 15, 16, 16, 17, 17,
            17, 18, 18, 18, 19, 19, 20, 20, 21, 22, 23, 24, 26, 28, 31, 34, 38, 42, 47, 52, 58, 64, 68, 71,
            74, 76, 78, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 94, 96, 98, 100,
        ];

    /// <inheritdoc/>
    public bool IsCrafting() => this.dalamudServices.Condition[ConditionFlag.Crafting] && !this.dalamudServices.Condition[ConditionFlag.PreparingToCraft];

    /// <inheritdoc/>
    public bool IsNotCrafting() => !this.IsCrafting();

    /// <inheritdoc/>
    public unsafe bool IsCollectable()
    {
        var addon = this.GetSynthesisAddon();

        return addon->AtkUnitBase.UldManager.NodeList[34]->IsVisible();
    }

    /// <inheritdoc/>
    public unsafe string GetCondition(bool lower = true)
    {
        var addon = this.GetSynthesisAddon();

        var text = addon->Condition->NodeText.ToString();

        if (lower)
        {
            text = text.ToLowerInvariant();
        }

        return text;
    }

    /// <inheritdoc/>
    public bool HasCondition(string condition, bool lower = true)
    {
        var actual = this.GetCondition(lower);
        return condition == actual;
    }

    /// <inheritdoc/>
    public unsafe int GetProgress()
    {
        var addon = this.GetSynthesisAddon();
        return this.GetNodeTextAsInt(addon->CurrentProgress, "Could not parse current progress number in the Synthesis addon");
    }

    /// <inheritdoc/>
    public unsafe int GetMaxProgress()
    {
        var addon = this.GetSynthesisAddon();
        return this.GetNodeTextAsInt(addon->MaxProgress, "Could not parse max progress number in the Synthesis addon");
    }

    /// <inheritdoc/>
    public bool HasMaxProgress()
    {
        var current = this.GetProgress();
        var max = this.GetMaxProgress();
        return current == max;
    }

    /// <inheritdoc/>
    public unsafe int GetQuality()
    {
        var addon = this.GetSynthesisAddon();
        return this.GetNodeTextAsInt(addon->CurrentQuality, "Could not parse current quality number in the Synthesis addon");
    }

    /// <inheritdoc/>
    public unsafe int GetMaxQuality()
    {
        var addon = this.GetSynthesisAddon();
        return this.GetNodeTextAsInt(addon->MaxQuality, "Could not parse max quality number in the Synthesis addon");
    }

    /// <inheritdoc/>
    public bool HasMaxQuality()
    {
        var step = this.GetStep();

        if (step <= 1)
        {
            return false;
        }

        if (this.IsCollectable())
        {
            var current = this.GetQuality();
            var max = this.GetMaxQuality();
            return current == max;
        }

        var percentHq = this.GetPercentHQ();
        return percentHq == 100;
    }

    /// <inheritdoc/>
    public unsafe int GetDurability()
    {
        var addon = this.GetSynthesisAddon();
        return this.GetNodeTextAsInt(addon->CurrentDurability, "Could not parse current durability number in the Synthesis addon");
    }

    /// <inheritdoc/>
    public unsafe int GetMaxDurability()
    {
        var addon = this.GetSynthesisAddon();
        return this.GetNodeTextAsInt(addon->StartingDurability, "Could not parse max durability number in the Synthesis addon");
    }

    /// <inheritdoc/>
    public int GetCp()
    {
        var cp = this.dalamudServices.ClientState.LocalPlayer?.CurrentCp ?? 0;
        return (int)cp;
    }

    /// <inheritdoc/>
    public int GetMaxCp()
    {
        var cp = this.dalamudServices.ClientState.LocalPlayer?.MaxCp ?? 0;
        return (int)cp;
    }

    /// <inheritdoc/>
    public unsafe int GetStep()
    {
        var addon = this.GetSynthesisAddon();
        var step = this.GetNodeTextAsInt(addon->StepNumber, "Could not parse current step number in the Synthesis addon");
        return step;
    }

    /// <inheritdoc/>
    public unsafe int GetPercentHQ()
    {
        var addon = this.GetSynthesisAddon();
        if (!addon->AtkUnitBase.UldManager.NodeList[33]->IsVisible())
        {
            var current = this.GetNodeTextAsInt(addon->CurrentQuality, "Could not parse current quality number in the Synthesis addon");
            var max = this.GetNodeTextAsInt(addon->MaxQuality, "Could not parse max quality number in the Synthesis addon");
            return HqPercentTable[(int)Math.Clamp((float)current / max * 100, 0, 100)];
        }

        var percentHq = this.GetNodeTextAsInt(addon->HQPercentage, "Could not parse percent hq number in the Synthesis addon");
        return percentHq;
    }

    /// <inheritdoc/>
    public unsafe void OpenRecipeByRecipeId(uint recipeId) => ((AgentRecipeNote*)Framework.Instance()->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.RecipeNote))->OpenRecipeByRecipeId(recipeId);

    /// <inheritdoc/>
    public unsafe uint? GetRecipeNoteSelectedRecipeId()
    {
        var list = RecipeNote.Instance()->RecipeList;
        if (list == null)
        {
            return null;
        }

        var index = ((ushort*)((byte*)list + 0x428))[0];
        var selection = list->Recipes + index;

        if (selection == null)
        {
            return null;
        }

        return selection->RecipeId;
    }
}