// <copyright file="Cheat.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Cheat;

/// <summary>
///     Cheat interface.
/// </summary>
internal abstract class Cheat
{
    /// <summary>
    ///     Gets a value indicating whether the cheat is enabled.
    /// </summary>
    public abstract bool Enabled { get; }

    /// <summary>
    ///     Think that the cheat should do on enable.
    /// </summary>
    /// <returns>Enabling was a success.</returns>
    public abstract bool OnEnabled();

    /// <summary>
    ///     Think that the cheat should do on disable.
    /// </summary>
    public abstract void OnDisabled();

    public virtual void OnTerritoryChange(ushort e)
    {
    }
}