// <copyright file="Tab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.UI;

using Athavar.FFXIV.Plugin.Models.Interfaces;

public abstract class Tab : IDisposable, ITab
{
    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract string Identifier { get; }

    /// <inheritdoc/>
    public virtual string Title => this.Name;

    /// <inheritdoc/>
    public bool Enabled { get; set; } = true;

    /// <inheritdoc/>
    public virtual void Dispose()
    {
    }

    /// <inheritdoc/>
    public abstract void Draw();

    /// <inheritdoc/>
    public virtual void OnNotDraw()
    {
    }
}