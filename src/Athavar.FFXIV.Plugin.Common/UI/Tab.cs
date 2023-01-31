// <copyright file="ITab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.UI;

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