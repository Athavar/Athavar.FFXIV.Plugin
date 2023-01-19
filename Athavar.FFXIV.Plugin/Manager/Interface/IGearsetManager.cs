// <copyright file="IGearsetManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Manager.Interface;

using System.Collections.Generic;
using Athavar.FFXIV.Plugin.Utils;

/// <summary>
///     Manager to manger the <see cref="Gearset" /> of the player.
/// </summary>
internal interface IGearsetManager
{
    /// <summary>
    ///     Gets all <see cref="Gearset" />.
    /// </summary>
    public IEnumerable<Gearset> AllGearsets { get; }

    /// <summary>
    ///     Updates all <see cref="Gearset" />.
    /// </summary>
    public void UpdateGearsets();
}