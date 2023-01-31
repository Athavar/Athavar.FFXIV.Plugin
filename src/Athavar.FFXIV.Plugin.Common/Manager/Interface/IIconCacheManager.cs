// <copyright file="IconCacheManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

using System.Diagnostics.CodeAnalysis;
using ImGuiScene;

/// <summary>
///     Cache for icons.
/// </summary>
public interface IIconCacheManager
{
    /// <summary>
    ///     Gets an icon.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="hq">If the icon should be hq.</param>
    /// <returns><see cref="TextureWrap" /> to draw the icon.</returns>
    public TextureWrap? GetIcon(uint iconId, bool hq = false);

    /// <summary>
    ///     Try to gets an icon.
    /// </summary>
    /// <param name="iconId">The icon Id.</param>
    /// <param name="hq">If the icon should be hq.</param>
    /// <param name="textureWrap"><see cref="TextureWrap" /> to draw the icon.</param>
    /// <returns>returns a value indication if the icon exists.</returns>
    bool TryGetIcon(uint iconId, bool hq, [NotNullWhen(true)] out TextureWrap? textureWrap);
}