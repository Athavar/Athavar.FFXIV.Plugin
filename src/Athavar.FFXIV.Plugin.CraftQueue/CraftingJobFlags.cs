namespace Athavar.FFXIV.Plugin.CraftQueue;

[Flags]
public enum CraftingJobFlags
{
    /// <summary>
    ///     the Zero state.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Force usage of food.
    /// </summary>
    ForceFood = 1 << 0,

    /// <summary>
    ///     Force usage of potion.
    /// </summary>
    ForcePotion = 1 << 1,
}