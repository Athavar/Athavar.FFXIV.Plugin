// <copyright file="ActionEffectParser.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;

using System.Text;
using Athavar.FFXIV.Plugin.Common.Definitions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Lumina.Excel.GeneratedSheets;

internal static class ActionEffectParser
{
    public static string DescribeFields(ActionEffect eff, IDalamudServices dalamudServices)
    {
        string StatusString(uint statusId)
        {
            var statusData = dalamudServices.DataManager.GetExcelSheet<Status>()?.GetRow(statusId);
            var name = statusData?.Name ?? "<not found>";
            return $"{statusId} '{name}'";
        }

        // note: for all effects, bit 7 of param4 means "applied to caster instead of target"
        var res = new StringBuilder();
        switch (eff.Type)
        {
            case ActionEffectType.Damage:
            case ActionEffectType.BlockedDamage:
            case ActionEffectType.ParriedDamage:
                // param0: bit 0 = crit, bit 1 = direct hit, others unused
                // param1: damage/element type
                // param2: bonus percent in log (purely visual)
                // param3: third (high) byte of value (if bit 6 in param4 is set), 0 otherwise
                // param4: bit1 = ? (seen when part of damage is absorbed by BLM manaward), bit2 = partial absorb? (seen when part of damage is absorbed by SMN succor), bit4 = immune (e.g. because of transcendent after raise),
                //         bit5 = retaliation (set together with bit 7, e.g. for damage from vengeance), bit 6 = large value, bit 7 = applied to source, others unused
                res.Append($"amount={eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0)} {(DamageType)(eff.Param1 & 0x0F)} {(DamageElementType)(eff.Param1 >> 4)} ({(sbyte)eff.Param2}% bonus)");
                if ((eff.Param0 & 1) != 0)
                {
                    res.Append(", crit");
                }

                if ((eff.Param0 & 2) != 0)
                {
                    res.Append(", dhit");
                }

                if ((eff.Param4 & 2) != 0)
                {
                    res.Append(", manaward absorb?");
                }

                if ((eff.Param4 & 4) != 0)
                {
                    res.Append(", partially absorbed?");
                }

                if ((eff.Param4 & 0x10) != 0)
                {
                    res.Append(", immune");
                }

                if ((eff.Param4 & 0x20) != 0)
                {
                    res.Append(", retaliation");
                }

                break;
            case ActionEffectType.Heal:
                // param0: bit 0 = lifedrain? (e.g. melee bloodbath, SCH energy drain, etc. - also called "absorb"), bit 1 = nascent flash?, others unused
                // param1: bit 0 = crit, others unused
                // param2: unused
                // param3: third (high) byte of value (if bit 6 in param4 is set), 0 otherwise
                // param4: bit 6 = large value, bit 7 = applied to source, others unused
                res.Append($"amount={eff.Value + ((eff.Param4 & 0x40) != 0 ? eff.Param3 * 0x10000 : 0)}");
                if ((eff.Param1 & 1) != 0)
                {
                    res.Append(", crit");
                }

                if ((eff.Param0 & 1) != 0)
                {
                    res.Append(", lifedrain?");
                }

                if ((eff.Param0 & 2) != 0)
                {
                    res.Append(", nascent flash?");
                }

                break;
            case ActionEffectType.Invulnerable:
                // value: either 0 or status id
                if (eff.Value != 0)
                {
                    res.Append($"status {StatusString(eff.Value)}");
                }

                break;
            case ActionEffectType.MpGain:
            case ActionEffectType.TpGain:
                res.Append($"amount={eff.Value}");
                break;
            case ActionEffectType.ApplyStatusEffectTarget:
            case ActionEffectType.ApplyStatusEffectSource:
                // param0/1: ??? (seen full range of values)
                // param2: low byte of 'extra' (don't know where high byte is...)
                // param3: unused
                // param4: bit5 = retaliation, bit 7 = applied to source, others unused
                res.Append($"{StatusString(eff.Value)} (xx{eff.Param2:X2})");
                if ((eff.Param4 & 0x20) != 0)
                {
                    res.Append(", retaliation");
                }

                break;
            case ActionEffectType.RecoveredFromStatusEffect:
                // param0: low byte of 'extra' (don't know where high byte is...)
                // param1-4: unused (except source bit in param4)
                res.Append($"{StatusString(eff.Value)} (xx{eff.Param0:X2})");
                break;
            case ActionEffectType.LoseStatusEffectTarget:
            case ActionEffectType.LoseStatusEffectSource:
                res.Append(StatusString(eff.Value));
                break;
            case ActionEffectType.Knockback:
                var kbData = dalamudServices.DataManager.GetExcelSheet<Knockback>()?.GetRow(eff.Value);
                res.Append($"row={eff.Value}, dist={kbData?.Distance}+{eff.Param0}, dir={(KnockbackDirection?)kbData?.Direction}{(kbData?.Direction == (byte)KnockbackDirection.Arg ? $" ({kbData.DirectionArg}deg)" : "")}, speed={kbData?.Speed}");
                break;
            case ActionEffectType.Attract1:
            case ActionEffectType.Attract2:
                res.Append($"row={eff.Value}, TODO lumina...");
                break;
            case ActionEffectType.AttractCustom1:
            case ActionEffectType.AttractCustom2:
            case ActionEffectType.AttractCustom3:
                res.Append($"dist={eff.Value} (min={eff.Param1}), speed={eff.Param0}");
                break;
            case ActionEffectType.SetHP:
                res.Append($"value={eff.Value}");
                break;
        }

        return res.ToString();
    }

    public static string DescribeUnknown(ActionEffect eff)
    {
        switch (eff.Type)
        {
            case ActionEffectType.Miss:
            case ActionEffectType.FullResist:
            case ActionEffectType.NoEffectText: // e.g. taunt immune
            case ActionEffectType.FailMissingStatus: // e.g. deployment tactics or bane when target doesn't have required status
                // so far never seen any non-zero params
                return eff.Param0 != 0 || eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 || eff.Value != 0 ? "non-zero params" : "";
            case ActionEffectType.Damage:
            case ActionEffectType.BlockedDamage:
            case ActionEffectType.ParriedDamage:
                if ((eff.Param0 & ~3) != 0)
                {
                    return $"param0={eff.Param0 & ~3:X2}";
                }

                if (eff.Param3 != 0 && (eff.Param4 & 0x40) == 0)
                {
                    return "non-zero param3 while large-value bit is unset";
                }

                if ((eff.Param4 & ~0xF0) != 0)
                {
                    return $"param4={eff.Param4 & ~0xF0:X2}";
                }

                if ((eff.Param4 & 0x10) != 0 && eff.Value != 0)
                {
                    return "immune bit set but value is non-zero";
                }

                if ((eff.Param4 & 0xA0) == 0x20)
                {
                    return "retaliation bit set but source is unset";
                }

                return "";
            case ActionEffectType.Heal:
                if ((eff.Param0 & ~3) != 0)
                {
                    return $"param0={eff.Param0 & ~3:X2}";
                }

                if ((eff.Param1 & ~1) != 0)
                {
                    return $"param1={eff.Param1 & ~1:X2}";
                }

                if (eff.Param2 != 0)
                {
                    return $"param2={eff.Param2}";
                }

                if (eff.Param3 != 0 && (eff.Param4 & 0x40) == 0)
                {
                    return "non-zero param3 while large-value bit is unset";
                }

                if ((eff.Param4 & ~0xC0) != 0)
                {
                    return $"param4={eff.Param4 & ~0xC0:X2}";
                }

                if (eff.Param0 != 0 && (eff.Param4 & 0x80) == 0)
                {
                    return "lifedrain bits set while source bit is unset";
                }

                return "";
            case ActionEffectType.Invulnerable:
            case ActionEffectType.MpGain:
            case ActionEffectType.TpGain:
            case ActionEffectType.LoseStatusEffectTarget:
            case ActionEffectType.LoseStatusEffectSource:
            case ActionEffectType.SetHP:
                // so far only seen 'source' flag and non-zero values
                return eff.Param0 != 0 || eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || (eff.Param4 & ~0x80) != 0 ? "non-zero params" : "";
            case ActionEffectType.ApplyStatusEffectTarget:
            case ActionEffectType.ApplyStatusEffectSource:
                if (eff.Param3 != 0 || (eff.Param4 & ~0xA0) != 0)
                {
                    return "non-zero param3/4";
                }

                if ((eff.Param4 & 0xA0) == 0x20)
                {
                    return "retaliation bit set but source is unset";
                }

                return "TODO investigate param0/1"; // $"{Utils.StatusString(eff.Value)} {eff.Param0:X2}{eff.Param1:X2}"; - these are often non-zero, but I have no idea what they mean...
            case ActionEffectType.RecoveredFromStatusEffect:
                return eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || (eff.Param4 & ~0x80) != 0 ? "non-zero params" : "";
            case ActionEffectType.Knockback:
                return eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 ? "non-zero params" : "";
            case ActionEffectType.Attract1:
            case ActionEffectType.Attract2:
                return eff.Param0 != 0 || eff.Param1 != 0 || eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 ? "non-zero params" : "";
            case ActionEffectType.AttractCustom1:
            case ActionEffectType.AttractCustom2:
            case ActionEffectType.AttractCustom3:
                return eff.Param2 != 0 || eff.Param3 != 0 || eff.Param4 != 0 ? "non-zero params" : "";
            default:
                return "unknown type";
        }
    }
}