// <copyright file="IIconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using System.Diagnostics.CodeAnalysis;
using Dalamud.Interface.Textures;
using ImGuiScene;

/// <summary>
///     Cache for icons.
/// </summary>
public interface IIconManager
{
    /// <summary>
    ///     Get a <see cref="T:ImGuiScene.TextureWrap"/> containing the icon with the given ID.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <returns><see cref="TextureWrap"/> to draw the icon.</returns>
    ISharedImmediateTexture? GetIcon(uint iconId);

    /// <summary>
    ///     Try to get a <see cref="T:ImGuiScene.TextureWrap"/> containing the icon with the given ID.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="textureWrap"><see cref="TextureWrap"/> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryGetIcon(uint iconId, [NotNullWhen(true)] out ISharedImmediateTexture? textureWrap);

    /// <summary>
    ///     Get a <see cref="T:ImGuiScene.TextureWrap"/> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="gameIconLookup">The icon lookup data.</param>
    /// <returns><see cref="TextureWrap"/> to draw the icon.</returns>
    ISharedImmediateTexture? GetIcon(GameIconLookup gameIconLookup);

    /// <summary>
    ///     Try to get a <see cref="T:ImGuiScene.TextureWrap"/> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="gameIconLookup">The icon lookup data.</param>
    /// <param name="textureWrap"><see cref="TextureWrap"/> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryGetIcon(GameIconLookup gameIconLookup, [NotNullWhen(true)] out ISharedImmediateTexture? textureWrap);

    ISharedImmediateTexture? GetJobIcon(Job job, JobIconStyle style = JobIconStyle.Normal, bool hr = false);

    bool TryGetJobIcon(Job job, JobIconStyle style, bool hr, [NotNullWhen(true)] out ISharedImmediateTexture? textureWrap);
}