// <copyright file="IClick.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click;

/// <summary>
///     Defines methods to send clicks to FFXIV UI elements.
/// </summary>
public interface IClick
{
    /// <summary>
    ///     Send a click by the name of the individual click.
    /// </summary>
    /// <param name="name">Click name.</param>
    /// <param name="addon">Pointer to an existing addon.</param>
    void SendClick(string name, nint addon = default);

    /// <summary>
    ///     Send a click by the name of the individual click.
    /// </summary>
    /// <param name="name">Click name.</param>
    /// <param name="addon">Pointer to an existing addon.</param>
    /// <returns>A value indicating whether the delegate was successfully called.</returns>
    bool TrySendClick(string name, nint addon = default);

    /// <summary>
    ///     Get a list of available click strings that can be used with SendClick.
    /// </summary>
    /// <returns>A list of click names.</returns>
    public IList<string> GetClickNames();
}