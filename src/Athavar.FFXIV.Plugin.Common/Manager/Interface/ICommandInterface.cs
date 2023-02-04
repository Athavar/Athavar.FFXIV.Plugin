// <copyright file="ICommandInterface.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

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
}