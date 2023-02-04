// <copyright file="Module.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Common.UI;

/// <summary>
///     Basic property and methods of a module.
/// </summary>
public abstract class Module
{
    protected Module(Configuration configuration) => this.Configuration = configuration;

    public Configuration Configuration { get; }

    /// <summary>
    ///     Gets the name of the module.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets a value indicating whether the plugin is hidden.
    /// </summary>
    public virtual bool Hidden { get; } = false;

    /// <summary>
    ///     Gets the tab ui.
    /// </summary>
    public virtual ITab? Tab { get; } = null;

    /// <summary>
    ///     Gets a value indicating whether gets a value indication the module is enabled or not.
    /// </summary>
    public bool Enabled => this.GetEnableStateAction().Get(this.Configuration);

    /// <summary>
    ///     Enable/Disable the module.
    /// </summary>
    /// <param name="state">A value indication if the module should be enabled or not.</param>
    public void Enable(bool state = true) => this.GetEnableStateAction().Set(state, this.Configuration);

    /// <summary>
    ///     Gets the Functions to get and set the enable state of a module.
    /// </summary>
    /// <returns>return a getter and setter.</returns>
    public abstract (Func<Configuration, bool> Get, Action<bool, Configuration> Set) GetEnableStateAction();
}