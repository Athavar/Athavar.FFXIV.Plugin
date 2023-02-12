// <copyright file="ICommandInterface.State.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

public partial interface ICommandInterface
{
    /// <summary>
    ///     Gets the Id of the current territory.
    /// </summary>
    /// <returns>returns the territory row id.</returns>
    ushort GetCurrentTerritory();

    /// <summary>
    ///     Get the name of the current selected target.
    /// </summary>
    /// <returns>The name of the current selected target.</returns>
    public string? GetCurrentTarget();

    /// <summary>
    ///     Get the name of the current selected target.
    /// </summary>
    /// <returns>The name of the current selected target.</returns>
    public string? GetCurrentFocusTarget();

    /// <summary>
    ///     Gets the Id of the current Job.
    /// </summary>
    /// <returns>returns the classjob row id.</returns>
    public byte GetCurrentJob();

    /// <summary>
    ///     Gets a value indicating whether the given status is present on the player.
    /// </summary>
    /// <param name="statusName">Status name.</param>
    /// <returns>A value indicating whether the given status is present on the player.</returns>
    public bool HasStatus(string statusName);

    /// <summary>
    ///     Gets a value indicating whether the given status is present on the player.
    /// </summary>
    /// <param name="statusIDs">Status IDs.</param>
    /// <returns>A value indicating whether the given status is present on the player.</returns>
    public bool HasStatusId(params uint[] statusIDs);

    /// <summary>
    ///     Checks if the character is in combat.
    /// </summary>
    /// <returns>A value indication whether the player is in combat.</returns>
    bool IsInCombat();

    /// <summary>
    ///     Checks if the character is in a duty.
    /// </summary>
    /// <returns>A value indication whether the player is in a duty.</returns>
    bool IsInDuty();

    /// <summary>
    ///     Checks if the character is performing.
    /// </summary>
    /// <returns>A value indication whether the player is performing.</returns>
    bool IsPerforming();

    /// <summary>
    ///     Checks if the character is in golden saucer.
    /// </summary>
    /// <returns>A value indication whether the player is in golden saucer.</returns>
    bool IsInGoldenSaucer();

    /// <summary>
    ///     Checks if the character is in a PvP area.
    /// </summary>
    /// <returns>A value indication whether the player is in a pvp area.</returns>
    bool IsPvP();

    /// <summary>
    ///     Log the current logged in character out.
    /// </summary>
    /// <returns>A value indication whether the logout command could be executed.</returns>
    bool LogOut();

    /// <summary>
    ///     Checks if the player character is ready and not moving between zones.
    /// </summary>
    /// <returns>A value indication whether the player character is ready.</returns>
    bool IsPlayerCharacterReady();
}