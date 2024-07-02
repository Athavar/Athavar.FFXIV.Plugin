// <copyright file="IIpcManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using Athavar.FFXIV.Plugin.Models.Constants;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;

public interface IIpcManager
{
    /// <summary>
    ///     Gets the glamourer api version.
    /// </summary>
    (int Breaking, int Features) GlamourerApiVersion { get; }

    /// <summary>
    ///     Gets a value indicating whether glamourer is enabled or not.
    /// </summary>
    bool GlamourerEnabled { get; }

    /// <summary>
    ///     Sets an item on a character.
    /// </summary>
    /// <param name="character">The <see cref="Character"/>.</param>
    /// <param name="slot">The equipment slot.</param>
    /// <param name="itemId">The id of the item.</param>
    /// <param name="stainId">The id of the strain/color.</param>
    /// <param name="key">Any Key.</param>
    /// <returns>if is was successful.</returns>
    int SetItem(IPlayerCharacter? character, EquipSlot slot, ulong itemId, byte stainId, uint key);

    /// <summary>
    ///     Update Active Plugin State.
    /// </summary>
    void UpdateActivePluginState();
}