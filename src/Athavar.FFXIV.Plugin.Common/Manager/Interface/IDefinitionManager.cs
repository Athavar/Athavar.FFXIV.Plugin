// <copyright file="IDefinitionManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using Athavar.FFXIV.Plugin.Common.Definitions;
using Dalamud;

public interface IDefinitionManager
{
    /// <summary>
    ///     Gets the <see cref="DalamudStartInfo" /> of dalamud.
    /// </summary>
    DalamudStartInfo StartInfo { get; }

    /// <summary>
    ///     Gets an <see cref="Action" /> by Id.
    /// </summary>
    /// <param name="actionId">The <see cref="Action" /> id.</param>
    /// <returns>return the <see cref="Action" />.</returns>
    Action? GetActionById(uint actionId);

    /// <summary>
    ///     Gets a <see cref="StatusEffect" /> by Id.
    /// </summary>
    /// <param name="statusId">The <see cref="StatusEffect" /> id.</param>
    /// <returns>return the <see cref="StatusEffect" />.</returns>
    StatusEffect? GetStatusEffectById(uint statusId);

    /// <summary>
    ///     Gets all StatusIds that have the given reactive Proc types.
    /// </summary>
    /// <param name="procType">the <see cref="ReactiveProc.ReactiveProcType" />.</param>
    /// <returns>returns the status ids.</returns>
    uint[] GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType procType);
}