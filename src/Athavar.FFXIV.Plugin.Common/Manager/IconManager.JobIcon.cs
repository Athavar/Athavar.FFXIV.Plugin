// <copyright file="IconManager.JobIcon.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Models;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;

internal sealed partial class IconManager
{
    private static readonly uint[] JobIconStyleOffset = { 62000, 62100, 62800, 62300, 91000, 91500, 92000, 92500, 93000, 93500, 94000, 94500 };

    public IDalamudTextureWrap? GetJobIcon(Job job, JobIconStyle style = JobIconStyle.Normal, bool hr = false)
    {
        uint ResolveJobIconId(Job j, JobIconStyle s)
        {
            switch (j)
            {
                case Job.LimitBreak:
                    return 103;
                default:
                {
                    var id = ResolveJobOrder(j, s);
                    if (id == 0)
                    {
                        id = ResolveJobOrder(j, JobIconStyle.Normal);
                    }

                    return JobIconStyleOffset[(int)s] + id;
                }
            }
        }

        uint ResolveJobOrder(Job j, JobIconStyle s)
            => s switch
               {
                   JobIconStyle.Quest => JobQuestOrder(j),
                   JobIconStyle.White => FancyJobOrder(j),
                   JobIconStyle.Black => FancyJobOrder(j),
                   JobIconStyle.Yellow => FancyJobOrder(j),
                   JobIconStyle.Orange => FancyJobOrder(j),
                   JobIconStyle.Red => FancyJobOrder(j),
                   JobIconStyle.Blue => FancyJobOrder(j),
                   JobIconStyle.Purple => FancyJobOrder(j),
                   JobIconStyle.Green => FancyJobOrder(j),
                   _ => NormalJobOrder(j),
               };

        uint NormalJobOrder(Job j)
            => j switch
               {
                   Job.Chocobo => 41,
                   Job.Pets => 42,
                   _ => (uint)j,
               };

        uint JobQuestOrder(Job j)
            => j switch
               {
                   Job.Adventurer => 0,
                   Job.Gladiator => 1,
                   Job.Pugilist => 2,
                   Job.Marauder => 3,
                   Job.Lancer => 4,
                   Job.Archer => 5,
                   Job.Conjurer => 6,
                   Job.Thaumaturge => 7,
                   Job.Carpenter => 10,
                   Job.Blacksmith => 11,
                   Job.Armorer => 12,
                   Job.Goldsmith => 13,
                   Job.Leatherworker => 14,
                   Job.Weaver => 15,
                   Job.Alchemist => 16,
                   Job.Culinarian => 17,
                   Job.Miner => 18,
                   Job.Botanist => 19,
                   Job.Fisher => 20,
                   Job.Paladin => 101,
                   Job.Monk => 102,
                   Job.Warrior => 103,
                   Job.Dragoon => 104,
                   Job.Bard => 105,
                   Job.WhiteMage => 106,
                   Job.BlackMage => 107,
                   Job.Arcanist => 8,
                   Job.Summoner => 108,
                   Job.Scholar => 109,
                   Job.Rogue => 9,
                   Job.Ninja => 110,
                   Job.Machinist => 111,
                   Job.DarkKnight => 112,
                   Job.Astrologian => 113,
                   Job.Samurai => 114,
                   Job.RedMage => 115,
                   Job.BlueMage => 116,
                   Job.Gunbreaker => 117,
                   Job.Dancer => 118,
                   Job.Reaper => 119,
                   Job.Sage => 120,
                   Job.Pets => 0,
                   Job.Chocobo => 0,
                   Job.LimitBreak => 0,
                   _ => throw new ArgumentOutOfRangeException(nameof(job), job, null),
               };

        uint FancyJobOrder(Job j)
            => j switch
               {
                   Job.Adventurer => 0,
                   Job.Gladiator => 22,
                   Job.Pugilist => 23,
                   Job.Marauder => 24,
                   Job.Lancer => 25,
                   Job.Archer => 26,
                   Job.Conjurer => 28,
                   Job.Thaumaturge => 29,
                   Job.Carpenter => 31,
                   Job.Blacksmith => 32,
                   Job.Armorer => 33,
                   Job.Goldsmith => 34,
                   Job.Leatherworker => 35,
                   Job.Weaver => 36,
                   Job.Alchemist => 37,
                   Job.Culinarian => 38,
                   Job.Miner => 39,
                   Job.Botanist => 40,
                   Job.Fisher => 41,
                   Job.Paladin => 79,
                   Job.Monk => 80,
                   Job.Warrior => 81,
                   Job.Dragoon => 82,
                   Job.Bard => 83,
                   Job.WhiteMage => 84,
                   Job.BlackMage => 85,
                   Job.Arcanist => 30,
                   Job.Summoner => 86,
                   Job.Scholar => 87,
                   Job.Rogue => 121,
                   Job.Ninja => 122,
                   Job.Machinist => 125,
                   Job.DarkKnight => 123,
                   Job.Astrologian => 124,
                   Job.Samurai => 127,
                   Job.RedMage => 128,
                   Job.BlueMage => 129,
                   Job.Gunbreaker => 130,
                   Job.Dancer => 131,
                   Job.Reaper => 132,
                   Job.Sage => 133,
                   Job.Chocobo => 42,
                   Job.Pets => 98,
                   Job.LimitBreak => 0,
                   _ => throw new ArgumentOutOfRangeException(nameof(job), job, null),
               };

        return this.GetIcon(ResolveJobIconId(job, style), ITextureProvider.IconFlags.HiRes) ?? this.GetIcon(ResolveJobIconId(job, JobIconStyle.Normal), ITextureProvider.IconFlags.HiRes);
    }

    /// <inheritdoc />
    public bool TryGetJobIcon(Job job, JobIconStyle style, bool hr, [NotNullWhen(true)] out IDalamudTextureWrap? textureWrap)
    {
        textureWrap = this.GetJobIcon(job, style, hr);
        return textureWrap is not null;
    }
}