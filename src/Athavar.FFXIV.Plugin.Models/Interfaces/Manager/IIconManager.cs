// <copyright file="IIconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces.Manager;

using System.Diagnostics.CodeAnalysis;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using ImGuiScene;

/// <summary>
///     Cache for icons.
/// </summary>
public interface IIconManager
{
    /// <summary>
    ///     Get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <returns><see cref="TextureWrap" /> to draw the icon.</returns>
    IDalamudTextureWrap? GetIcon(uint iconId);

    /// <summary>
    ///     Try to get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="textureWrap"><see cref="TextureWrap" /> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryGetIcon(uint iconId, [NotNullWhen(true)] out IDalamudTextureWrap? textureWrap);

    /// <summary>
    ///     Get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="flags">A value indicating whether the icon should be HQ.</param>
    /// <returns><see cref="TextureWrap" /> to draw the icon.</returns>
    IDalamudTextureWrap? GetIcon(uint iconId, ITextureProvider.IconFlags flags);

    /// <summary>
    ///     Try to get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="flags">A value indicating whether the icon should be HQ.</param>
    /// <param name="textureWrap"><see cref="TextureWrap" /> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryGetIcon(uint iconId, ITextureProvider.IconFlags flags, [NotNullWhen(true)] out IDalamudTextureWrap? textureWrap);

    IDalamudTextureWrap? GetJobIcon(Job job, JobIconStyle style = JobIconStyle.Normal, bool hr = false);

    bool TryGetJobIcon(Job job, JobIconStyle style, bool hr, [NotNullWhen(true)] out IDalamudTextureWrap? textureWrap);
}