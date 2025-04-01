// <copyright file="IDefinitionManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using Dalamud.Common;

public interface IDefinitionManager
{
    /// <summary>
    ///     Gets the <see cref="DalamudStartInfo"/> of dalamud.
    /// </summary>
    DalamudStartInfo StartInfo { get; }

    /// <summary>
    ///     Gets the name of a content.
    /// </summary>
    /// <param name="territoryId">The territoryID of the content.</param>
    /// <returns>Returns the name.</returns>
    MultiString GetContentName(uint territoryId);
}