// <copyright file="ICheat.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Cheat;

/// <summary>
///     Cheat interface.
/// </summary>
internal interface ICheat
{
    /// <summary>
    ///     Gets a value indicating whether the cheat is enabled.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    ///     Think that the cheat should do on enable.
    /// </summary>
    /// <returns>Enabling was a success.</returns>
    public bool OnEnabled();

    /// <summary>
    ///     Think that the cheat should do on disable.
    /// </summary>
    public void OnDisabled();
}