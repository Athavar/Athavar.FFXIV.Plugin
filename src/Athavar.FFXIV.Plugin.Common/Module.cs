// <copyright file="IModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Common.UI;

/// <summary>
///     Basic property and methods of a module.
/// </summary>
public abstract class Module
{
    /// <summary>
    ///     Gets the name of the module.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets a value indicating whether the plugin is hidden.
    /// </summary>
    public virtual bool Hidden { get; } = false;

    /// <summary>
    ///     Gets a value indication if the module is enabled or not.
    /// </summary>
    public abstract bool Enabled { get; }

    /// <summary>
    ///     Gets the tab ui.
    /// </summary>
    public virtual ITab? Tab { get; } = null;

    /// <summary>
    ///     Enable/Disable the module.
    /// </summary>
    /// <param name="state">A value indication if the module should be enabled or not.</param>
    public abstract void Enable(bool state = true);
}