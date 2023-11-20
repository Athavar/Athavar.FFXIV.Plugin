// <copyright file="Tab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.UI;

using Athavar.FFXIV.Plugin.Models.Interfaces;

public abstract class Tab : IDisposable, ITab
{
    /// <summary>
    ///     Gets the displayName of the tab.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets the identifier of the tab.
    /// </summary>
    public abstract string Identifier { get; }

    /// <summary>
    ///     Gets the title of the tab.
    /// </summary>
    public virtual string Title => this.Name;

    /// <inheritdoc />
    public virtual void Dispose()
    {
    }

    /// <summary>
    ///     Draw the content of the tab.
    /// </summary>
    public abstract void Draw();

    /// <summary>
    ///     Called if content of the tab is not draw.
    /// </summary>
    public virtual void OnNotDraw()
    {
    }
}