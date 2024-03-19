namespace Athavar.FFXIV.Plugin.Dps.Data.Encounter;

using Athavar.FFXIV.Plugin.Common.Definitions;

internal record StatusEntry(CombatEvent.StatusEffect Effect, StatusEffect? Definition = null);