// <copyright file="ITab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.UI;

public interface ITab
{
    /// <summary>
    ///     Gets the displayName of the tab.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the identifier of the tab.
    /// </summary>
    string Identifier { get; }

    /// <summary>
    ///     Gets the identifier of the tab.
    /// </summary>
    string Title { get; }

    /// <summary>
    ///     Draw the content of the tab.
    /// </summary>
    void Draw();

    /// <summary>
    ///     Called if content of the tab is not draw.
    /// </summary>
    void OnNotDraw();
}