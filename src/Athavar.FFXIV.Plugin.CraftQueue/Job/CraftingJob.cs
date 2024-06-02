namespace Athavar.FFXIV.Plugin.CraftQueue.Job;

using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.Models;

internal class CraftingJob(CraftQueue queue, RecipeExtended recipe, IRotationResolver rotationResolver, Gearset gearset, uint count, BuffConfig buffConfig, (uint ItemId, byte Amount)[] hqIngredients, CraftingJobFlags flags)
    : BaseCraftingJob(queue, recipe, rotationResolver, gearset, count, buffConfig, hqIngredients, flags);