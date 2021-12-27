// <copyright file="IModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

/// <summary>
/// Basic property and methods of a module.
/// </summary>
internal interface IModule
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Draw the main tab of the module.
    /// </summary>
    void Draw();

    /// <summary>
    /// Enable/Disable the module.
    /// </summary>
    /// <param name="state">A value indication if the module should be enabled or not.</param>
    void Enable(bool state = true);
}