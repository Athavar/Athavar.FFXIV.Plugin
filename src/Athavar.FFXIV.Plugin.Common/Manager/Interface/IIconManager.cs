// <copyright file="IIconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Config;
using ImGuiScene;

/// <summary>
///     Cache for icons.
/// </summary>
public interface IIconManager
{
    /// <summary>
    ///     Get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="isHq">A value indicating whether the icon should be HQ.</param>
    /// <returns><see cref="TextureWrap" /> to draw the icon.</returns>
    public TextureWrap? GetHqIcon(uint iconId, bool isHq = false);

    /// <summary>
    ///     Try to get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="isHq">A value indicating whether the icon should be HQ.</param>
    /// <param name="textureWrap"><see cref="TextureWrap" /> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryHqGetIcon(uint iconId, bool isHq, [NotNullWhen(true)] out TextureWrap? textureWrap);

    /// <summary>
    ///     Clean up loaded icon data.
    /// </summary>
    void CleanIconData();

    /// <summary>
    ///     Get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <returns><see cref="TextureWrap" /> to draw the icon.</returns>
    TextureWrap? GetIcon(uint iconId);

    /// <summary>
    ///     Try to get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="textureWrap"><see cref="TextureWrap" /> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryGetIcon(uint iconId, [NotNullWhen(true)] out TextureWrap? textureWrap);

    /// <summary>
    ///     Get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="hr">A value indicating whether the icon should be HQ.</param>
    /// <returns><see cref="TextureWrap" /> to draw the icon.</returns>
    TextureWrap? GetIcon(uint iconId, bool hr);

    /// <summary>
    ///     Try to get a <see cref="T:ImGuiScene.TextureWrap" /> containing the icon with the given ID, of the given quality.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="hr">A value indicating whether the icon should be HQ.</param>
    /// <param name="textureWrap"><see cref="TextureWrap" /> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryGetIcon(uint iconId, bool hr, [NotNullWhen(true)] out TextureWrap? textureWrap);

    TextureWrap? GetJobIcon(Job job, JobIconStyle style = JobIconStyle.Normal, bool hr = false);

    bool TryGetJobIcon(Job job, JobIconStyle style, bool hr, [NotNullWhen(true)] out TextureWrap? textureWrap);
}