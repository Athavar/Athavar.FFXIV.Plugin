// <copyright file="AutoSpear.Logic.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using System.Linq;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Utils.SeFunctions;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Component.GUI;

internal partial class AutoSpear
{
    public override unsafe void Draw()
    {
        this.DrawFish(this._currentSpot, this._addon->Fish1, this._addon->Fish1Node, this._addon->FishLines, 0);
        this.DrawFish(this._currentSpot, this._addon->Fish2, this._addon->Fish2Node, this._addon->FishLines, 1);
        this.DrawFish(this._currentSpot, this._addon->Fish3, this._addon->Fish3Node, this._addon->FishLines, 2);
    }

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
        return !this.SpearfishingSpots.TryGetValue(id, out var spot) ? null : spot;
    }

    // Given the current spot we can read the spearfish window and correspond fish to their speed and size.
    // This may result in more than one fish, but does so rarely. Unknown attributes are seen as valid for any attribute.
    private string Identify(FishingSpot? spot, SpearfishWindow.Info info)
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

    private unsafe void DrawFish(FishingSpot? spot, SpearfishWindow.Info info, AtkResNode* node, AtkResNode* fishLines, int index)
    {
        this.fishData[index] = $"Line {index}: Empty";
        if (!info.Available)
        {
            return;
        }

        var text = this.Identify(spot, info);


        var center = this._uiSize.X / 2;
        var posStart = node->X * this._uiScale;
        var posEnd = posStart + (node->Width * node->ScaleX * this._uiScale);

        this.fishData[index] = $"Line {index}: {center}, x1={(int)posStart}, x2={(int)posEnd}, Name: {text}";

        if (!Regex.IsMatch(text, this.configuration.FishMatchText))
        {
            return;
        }

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