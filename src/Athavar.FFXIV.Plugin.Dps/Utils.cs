// <copyright file="Utils.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

internal class Utils
{
    private readonly IDalamudServices dalamudServices;
    private readonly ExcelSheet<ClassJob> jobsSheet;

    public Utils(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;
        this.jobsSheet = this.dalamudServices.DataManager.GetExcelSheet<ClassJob>()!;
    }

    public static Vector2 GetAnchoredPosition(Vector2 position, Vector2 size, DrawAnchor anchor)
        => anchor switch
           {
               DrawAnchor.Center => position - (size / 2f),
               DrawAnchor.Left => position + new Vector2(0, -size.Y / 2f),
               DrawAnchor.Right => position + new Vector2(-size.X, -size.Y / 2f),
               DrawAnchor.Top => position + new Vector2(-size.X / 2f, 0),
               DrawAnchor.TopLeft => position,
               DrawAnchor.TopRight => position + new Vector2(-size.X, 0),
               DrawAnchor.Bottom => position + new Vector2(-size.X / 2f, -size.Y),
               DrawAnchor.BottomLeft => position + new Vector2(0, -size.Y),
               DrawAnchor.BottomRight => position + new Vector2(-size.X, -size.Y),
               _ => position,
           };

    public static string GetTagsTooltip(string[] textTags)
        => $"Available Text Tags:\n\n{string.Join("\n", textTags)}\n\n" +
           "Append the characters ':k' to a numeric tag to kilo-format it.\n" +
           "Append a '.' and a number to limit the number of characters,\n" +
           "or the number of decimals when used with numeric values.\n\nExamples:\n" +
           "[damagetotal]          =>    123,456\n" +
           "[damagetotal:k]      =>           123k\n" +
           "[damagetotal:k.1]  =>       123.4k\n\n" +
           "[name]                   =>    Firstname Lastname\n" +
           "[name_first.5]    =>    First\n" +
           "[name_last.1]     =>    L";

    public static bool IsJobType(Job job, JobType type, IEnumerable<Job>? jobList = null)
        => type switch
           {
               JobType.All => true,
               JobType.Tanks => job is Job.Gladiator or Job.Marauder or Job.Paladin or Job.Warrior or Job.DarkKnight or Job.Gunbreaker,
               JobType.Casters => job is Job.Thaumaturge or Job.Arcanist or Job.BlackMage or Job.Summoner or Job.RedMage or Job.BlueMage,
               JobType.Melee => job is Job.Pugilist or Job.Lancer or Job.Rogue or Job.Monk or Job.Dragoon or Job.Ninja or Job.Samurai or Job.Reaper,
               JobType.Ranged => job is Job.Archer or Job.Bard or Job.Machinist or Job.Dancer,
               JobType.Healers => job is Job.Conjurer or Job.WhiteMage or Job.Scholar or Job.Astrologian or Job.Sage,
               JobType.DoH => job is Job.Carpenter or Job.Blacksmith or Job.Armorer or Job.Goldsmith or Job.Leatherworker or Job.Weaver or Job.Alchemist or Job.Culinarian,
               JobType.DoL => job is Job.Miner or Job.Botanist or Job.Fisher,
               JobType.Combat => IsJobType(job, JobType.DoW) || IsJobType(job, JobType.DoM),
               JobType.DoW => IsJobType(job, JobType.Tanks) || IsJobType(job, JobType.Melee) || IsJobType(job, JobType.Ranged),
               JobType.DoM => IsJobType(job, JobType.Casters) || IsJobType(job, JobType.Healers),
               JobType.Crafters => IsJobType(job, JobType.DoH) || IsJobType(job, JobType.DoL),
               JobType.Custom => jobList is not null && jobList.Contains(job),
               _ => false,
           };

    public string ObjectString(GameObject obj) => $"{obj.DataId:X} '{obj.Name}' <{obj.ObjectId:X}>";

    public string ObjectString(ulong id)
    {
        var obj = id >> 32 == 0 ? this.dalamudServices.ObjectTable.SearchById((uint)id) : null;
        return obj != null ? this.ObjectString(obj) : $"(not founds) <{id:X}>";
    }

    public string StatusString(uint statusId)
    {
        var statusData = this.dalamudServices.DataManager.GetExcelSheet<Status>()?.GetRow(statusId);
        var name = statusData?.Name ?? "<not found>";
        return $"{statusId} '{name}'";
    }

    public string ActionString(uint actionId)
    {
        var statusData = this.dalamudServices.DataManager.GetExcelSheet<Action>()?.GetRow(actionId);
        var name = statusData?.Name ?? "<not found>";
        return $"{actionId} '{name}'";
    }

    public string ActionString(ActionId actionId) => this.ActionString(actionId.Id);

    public string JobName(Job input) => this.jobsSheet.GetRow((uint)input)?.Name.ToDalamudString().ToString() ?? string.Empty;

    public string Vec3String(Vector3 pos) => $"[{pos.X:f2}, {pos.Y:f2}, {pos.Z:f2}]";
}