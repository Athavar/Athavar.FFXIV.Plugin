// <copyright file="IGearsetManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using Athavar.FFXIV.Plugin.Common.Utils;

/// <summary>
///     Manager to manger the <see cref="Gearset" /> of the player.
/// </summary>
public interface IGearsetManager
{
    /// <summary>
    ///     Gets all <see cref="Gearset" />.
    /// </summary>
    public IEnumerable<Gearset> AllGearsets { get; }

    /// <summary>
    ///     Equip a gearset by id.
    /// </summary>
    /// <param name="gearsetId">The gearset id.</param>
    public void EquipGearset(int gearsetId);

    /// <summary>
    ///     Gets the current equipment as <see cref="Gearset" />.
    /// </summary>
    /// <returns>returns the current equipment as <see cref="Gearset" />.</returns>
    public Gearset? GetCurrentEquipment();

    /// <summary>
    ///     Updates all <see cref="Gearset" />.
    /// </summary>
    public void UpdateGearsets();
}