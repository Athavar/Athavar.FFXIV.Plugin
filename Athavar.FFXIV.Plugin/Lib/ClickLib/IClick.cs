namespace Athavar.FFXIV.Plugin.Lib.ClickLib;

using System;
using System.Collections.Generic;

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
    void SendClick(string name, IntPtr addon = default);

    /// <summary>
    ///     Send a click by the name of the individual click.
    /// </summary>
    /// <param name="name">Click name.</param>
    /// <param name="addon">Pointer to an existing addon.</param>
    /// <returns>A value indicating whether the delegate was successfully called.</returns>
    bool TrySendClick(string name, IntPtr addon = default);

    /// <summary>
    ///     Get a list of available click strings that can be used with SendClick.
    /// </summary>
    /// <returns>A list of click names.</returns>
    public IList<string> GetClickNames();
}