// <copyright file="Utils.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using System.Globalization;
using System.Numerics;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data;
using Athavar.FFXIV.Plugin.Dps.Data.ActionSummary;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.Sheets;

internal sealed class Utils
{
    private readonly Dictionary<uint, string> statusText = new();
    private readonly Dictionary<uint, string> actionText = new();
    private readonly IDalamudServices dalamudServices;
    private readonly ExcelSheet<ClassJob> jobsSheet;
    private readonly ExcelSheet<Status> statusTable;
    private readonly ExcelSheet<Action> actionTable;

    public Utils(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;
        this.jobsSheet = this.dalamudServices.DataManager.GetExcelSheet<ClassJob>()!;
        this.statusTable = this.dalamudServices.DataManager.GetExcelSheet<Status>()!;
        this.actionTable = this.dalamudServices.DataManager.GetExcelSheet<Action>()!;
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
            JobType.Casters => job is Job.Thaumaturge or Job.Arcanist or Job.BlackMage or Job.Summoner or Job.RedMage or Job.BlueMage or Job.Pictomancer,
            JobType.Melee => job is Job.Pugilist or Job.Lancer or Job.Rogue or Job.Monk or Job.Dragoon or Job.Ninja or Job.Samurai or Job.Reaper or Job.Viper,
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

    public string ObjectString(IGameObject obj) => $"{obj.DataId:X} '{obj.Name}' <{obj.EntityId:X}>";

    public string ObjectString(ulong id)
    {
        var obj = id >> 32 == 0 ? this.dalamudServices.ObjectTable.SearchById((uint)id) : null;
        return obj != null ? this.ObjectString(obj) : $"(not founds) <{id:X}>";
    }

    public string StatusString(uint statusId)
    {
        var statusData = this.dalamudServices.DataManager.GetExcelSheet<Status>().GetRowOrDefault(statusId);
        var name = statusData?.Name ?? "<not found>";
        return $"{statusId} '{name}'";
    }

    public string ActionString(uint actionId)
    {
        var statusData = this.dalamudServices.DataManager.GetExcelSheet<Action>().GetRowOrDefault(actionId);
        var name = statusData?.Name ?? "<not found>";
        return $"{actionId} '{name}'";
    }

    public string ActionString(ActionId actionId) => this.ActionString(actionId.Id);

    public string JobName(Job input) => this.jobsSheet.GetRowOrDefault((uint)input)?.Name.ToDalamudString().ToString() ?? string.Empty;

    public string Vec3String(Vector3 pos) => $"[{pos.X:f2}, {pos.Y:f2}, {pos.Z:f2}]";

    public void DrawActionSummaryTooltip(BaseCombatant combatant, bool force = false, MeterDataType? type = null)
    {
        const string defaultActionName = "Dot/Hot";
        if (!force && !ImGui.IsItemHovered())
        {
            return;
        }

        if (combatant is not Combatant c)
        {
            return;
        }

        string GetSummaryName(ActionSummary actionSummary)
        {
            if (actionSummary.Id == 0)
            {
                return defaultActionName;
            }

            string? name;
            if (actionSummary.IsStatus)
            {
                if (this.statusText.TryGetValue(actionSummary.Id, out name))
                {
                    return name;
                }

                name = $"[S]{this.statusTable.GetRowOrDefault(actionSummary.Id)?.Name.ToDalamudString().ToString() ?? string.Empty}";
                this.statusText.Add(actionSummary.Id, name);
                return name;
            }

            if (this.actionText.TryGetValue(actionSummary.Id, out name))
            {
                return name;
            }

            name = this.actionTable.GetRowOrDefault(actionSummary.Id)?.Name.ToDalamudString().ToString() ?? string.Empty;
            this.actionText.Add(actionSummary.Id, name);
            return name;
        }

        string Hits(double total, ulong part) => part == 0 ? "0" : $"{part} [{Pct(total, part)}]";

        string Pct(double total, ulong part) => $"{Math.Round((part / total) * 100, 2).ToString(CultureInfo.InvariantCulture)}%";

        void NumberWithAvg(ulong total, ulong hits) => ImGui.TextUnformatted($"{total.ToString(CultureInfo.InvariantCulture)} [~{total / hits}]");

        Func<ActionSummary, bool> actionSummaryFilter = type switch
        {
            MeterDataType.Damage => a => a.DamageDone is not null,
            MeterDataType.Healing => a => a.HealingDone is not null,
            MeterDataType.EffectiveHealing => a => a.HealingDone is not null,
            MeterDataType.DamageTaken => a => a.DamageTaken is not null,
            _ => a => a.DamageDone is not null || a.HealingDone is not null,
        };

        Func<ActionSummary, ulong> actionSummaryOrder = type switch
        {
            MeterDataType.Damage => a => a.DamageDone?.TotalAmount ?? 0,
            MeterDataType.Healing => a => a.HealingDone?.TotalAmount ?? 0,
            MeterDataType.EffectiveHealing => a => a.HealingDone?.TotalAmount - a.HealingDone?.OverAmount ?? 0,
            MeterDataType.DamageTaken => a => a.DamageTaken?.TotalAmount ?? 0,
            _ => a => a.Id,
        };

        var actionSummaries = c.Actions.Where(actionSummaryFilter).OrderByDescending(actionSummaryOrder).ToList();
        if (actionSummaries.Count == 0)
        {
            return;
        }

        var isHeal = type is MeterDataType.Healing or MeterDataType.EffectiveHealing;

        ImGui.BeginTooltip();

        if (ImGui.BeginTable("actionsummary", 6, ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.RowBg))
        {
            if (type == null)
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.WidthFixed, 150f);
                ImGui.TableSetupColumn("Hits/Swings", ImGuiTableColumnFlags.WidthStretch, 5 * 4.0f);
                ImGui.TableSetupColumn("Damage", ImGuiTableColumnFlags.WidthStretch, 5 * 12.0f);
                ImGui.TableSetupColumn("D. Hits", ImGuiTableColumnFlags.WidthStretch, 5 * 4.0f);
                ImGui.TableSetupColumn("Heal", ImGuiTableColumnFlags.WidthStretch, 5 * 12.0f);
                ImGui.TableSetupColumn("H. Hits", ImGuiTableColumnFlags.WidthStretch, 5 * 4.0f);
            }
            else
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.WidthFixed, 100f);
                ImGui.TableSetupColumn(type.Value.ToString(), ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.WidthFixed, 10 * 12.0f);
                ImGui.TableSetupColumn("Hits/Swings", ImGuiTableColumnFlags.WidthFixed, 15 * 4.0f);
                ImGui.TableSetupColumn("Crits", ImGuiTableColumnFlags.WidthFixed, 15 * 4.0f);
                ImGui.TableSetupColumn(isHeal ? "OverH" : "DHits", ImGuiTableColumnFlags.WidthFixed, 15 * 4.0f);
                ImGui.TableSetupColumn(isHeal ? "OverHPct" : "CritDHits", ImGuiTableColumnFlags.WidthFixed, 15 * 4.0f);
            }

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            if (type == null)
            {
                foreach (var summary in actionSummaries)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(GetSummaryName(summary));
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled(summary.Casts.ToString());
                    ImGui.TableNextColumn();
                    if (summary.DamageDone is { } damageDone)
                    {
                        NumberWithAvg(damageDone.TotalAmount, (ulong)damageDone.Hits);
                    }

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(summary.DamageDone?.Hits.ToString());
                    ImGui.TableNextColumn();
                    if (summary.HealingDone is { } healingDone)
                    {
                        NumberWithAvg(healingDone.TotalAmount, (ulong)healingDone.Hits);
                    }

                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(summary.HealingDone?.Hits.ToString());
                }
            }
            else
            {
                Func<ActionSummary, ActionTypeSummary> select = type switch
                {
                    MeterDataType.Damage => a => a.DamageDone!,
                    MeterDataType.Healing => a => a.HealingDone!,
                    MeterDataType.EffectiveHealing => a => a.HealingDone!,
                    MeterDataType.DamageTaken => a => a.DamageTaken!,
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
                };

                foreach (var summary in actionSummaries)
                {
                    var typeSummary = select(summary);

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(GetSummaryName(summary));
                    ImGui.TableNextColumn();
                    var total = typeSummary.TotalAmount;
                    if (type == MeterDataType.EffectiveHealing)
                    {
                        total -= typeSummary.OverAmount;
                    }

                    NumberWithAvg(total, (ulong)typeSummary.Hits);

                    ImGui.TableNextColumn();
                    ImGui.TextDisabled(typeSummary.Hits.ToString());
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(Hits(typeSummary.Hits, typeSummary.CritHits));
                    ImGui.TableNextColumn();
                    if (isHeal)
                    {
                        ImGui.TextUnformatted(typeSummary.OverAmount.ToString());
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(Pct(typeSummary.TotalAmount, typeSummary.OverAmount));
                    }
                    else
                    {
                        ImGui.TextUnformatted(Hits(typeSummary.Hits, typeSummary.DirectHits));
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(Hits(typeSummary.Hits, typeSummary.CritDirectHits));
                    }
                }
            }

            ImGui.EndTable();
        }

        ImGui.EndTooltip();
    }
}