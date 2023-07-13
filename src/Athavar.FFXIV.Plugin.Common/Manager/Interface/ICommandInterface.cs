// <copyright file="ICommandInterface.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using Dalamud.Game.ClientState.Objects.Enums;

/// <summary>
///     Miscellaneous functions that commands/scripts can use.
/// </summary>
public partial interface ICommandInterface
{
    /// <summary>
    ///     Check of an action can be used.
    /// </summary>
    /// <param name="actionId">The actionId.</param>
    /// <returns>A value indicating whether the action can be used.</returns>
    public bool CanUseAction(uint actionId);

    /// <summary>
    ///     Use an action by actionId.
    /// </summary>
    /// <param name="actionId">The actionId.</param>
    /// <returns>A value indicating whether the action was used.</returns>
    public bool UseAction(uint actionId);

    /// <summary>
    ///     Check of an general action can be used.
    /// </summary>
    /// <param name="actionId">The actionId.</param>
    /// <returns>A value indicating whether the general action can be used.</returns>
    public bool CanUseGeneralAction(uint actionId);

    /// <summary>
    ///     Use an general action by actionId.
    /// </summary>
    /// <param name="actionId">The actionId.</param>
    /// <returns>A value indicating whether the general action was used.</returns>
    public bool UseGeneralAction(uint actionId);

    /// <summary>
    ///     Check if the target in in interaction range.
    /// </summary>
    /// <param name="targetName">The name of the target.</param>
    /// <returns>A value indicating whether the target is in interaction reach.</returns>
    public bool IsTargetInReach(string targetName);

    /// <summary>
    ///     Check if the target in in interaction range.
    /// </summary>
    /// <param name="objectKind">The object kind of the target.</param>
    /// <param name="targetName">The name of the target.</param>
    /// <returns>A value indicating whether the target is in interaction reach.</returns>
    public bool IsTargetInReach(ObjectKind objectKind, string targetName);

    /// <summary>
    ///     Interact with a target.
    /// </summary>
    /// <param name="targetName">The name of the target.</param>
    /// <returns>A value indicating whether the interaction with the target was successful.</returns>
    public bool InteractWithTarget(string targetName);

    /// <summary>
    ///     Interact with a target.
    /// </summary>
    /// <param name="objectKind">The object kind of the target.</param>
    /// <param name="targetName">The name of the target.</param>
    /// <returns>A value indicating whether the interaction with the target was successful.</returns>
    public bool InteractWithTarget(ObjectKind objectKind, string targetName);
}