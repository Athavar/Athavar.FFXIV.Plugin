// <copyright file="AutoSpear.Logic.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.AutoSpear.Enum;
using Athavar.FFXIV.Plugin.AutoSpear.SeFunctions;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Component.GUI;

internal sealed partial class AutoSpear
{
    // We should always have to target a spearfishing spot when opening the window.
    // If we are not, hackery is afoot.
    private FishingSpot? GetTargetFishingSpot()
    {
        if (this.dalamudServices.TargetManager.Target == null)
        {
            return null;
        }

        if (this.dalamudServices.TargetManager.Target.ObjectKind != ObjectKind.GatheringPoint)
        {
            return null;
        }

        var id = this.dalamudServices.TargetManager.Target.DataId;
        return !this.spearfishingSpots.TryGetValue(id, out var spot) ? null : spot;
    }

    // Given the current spot we can read the spearfish window and correspond fish to their speed and size.
    // This may result in more than one fish, but does so rarely. Unknown attributes are seen as valid for any attribute.
    private string IdentifyFish(FishingSpot? spot, SpearfishWindow.Info info)
    {
        const string unknown = "Unknown Fish";

        if (spot == null)
        {
            return unknown;
        }

        var fishes = spot.Items.Where(f =>
                (f.Speed == info.Speed || f.Speed == SpearfishSpeed.Unknown)
             && (f.Size == info.Size || f.Size == SpearfishSize.Unknown))
           .ToList();
        return fishes.Count == 0 ? unknown : string.Join("\n", fishes.Select(f => f.Name[this.dalamudServices.DataManager.Language]));
    }

    private unsafe void CheckFish(FishingSpot? spot, SpearfishWindow.Info info, AtkResNode* node, int index)
    {
        this.fishData[index] = $"Line {index}: Empty";
        if (!info.Available)
        {
            return;
        }

        var text = this.IdentifyFish(spot, info);

        var center = this.uiSize.X / 2;
        var posStart = node->X * this.uiScale;
        var fishWide = node->Width * node->ScaleX * this.uiScale;
        var posEnd = posStart + fishWide;

        this.fishData[index] = $"Line {index}: {(int)center}, x1={(int)posStart}, x2={(int)posEnd}, Speed={info.Speed}, Size={info.Size}, Name: {text}";

        if (this.configuration.FishMatchText is null || !Regex.IsMatch(text, this.configuration.FishMatchText))
        {
            return;
        }

        var indexReduction = fishWide * 0.2f * (2 - index);
        posStart += indexReduction;
        posEnd -= indexReduction;

        if (!info.InverseDirection)
        {
            (posStart, posEnd) = (posEnd, posStart);
        }

        if (posStart < center && center < posEnd)
        {
            // catch
            if (this.dalamudServices.Condition[ConditionFlag.Gathering42])
            {
                return;
            }

            this.chatManager.SendMessage($"/ac \"{this.useSpearSkillName}\"");
        }
    }
}