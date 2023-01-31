// <copyright file="IconCacheManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Data;
using ImGuiScene;

/// <summary>
///     Cache for icons.
/// </summary>
internal class IconCacheManager : IIconCacheManager, IDisposable
{
    private readonly DataManager dataManager;
    private readonly Dictionary<(uint, bool), TextureWrap?> cache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconCacheManager" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    public IconCacheManager(IDalamudServices dalamudServices) => this.dataManager = dalamudServices.DataManager;

    /// <inheritdoc />
    public TextureWrap? GetIcon(uint iconId, bool hq)
    {
        if (this.cache.TryGetValue((iconId, hq), out var tex))
        {
            return tex;
        }

        tex = hq ? this.dataManager.GetImGuiTextureHqIcon(iconId) : this.dataManager.GetImGuiTextureIcon(iconId);
        this.cache.Add((iconId, hq), tex);
        return tex;
    }

    /// <inheritdoc />
    public bool TryGetIcon(uint iconId, bool hq, [NotNullWhen(true)] out TextureWrap? textureWrap)
    {
        textureWrap = this.GetIcon(iconId, hq);
        return textureWrap is not null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var textureWrap in this.cache)
        {
            textureWrap.Value?.Dispose();
        }

        this.cache.Clear();
    }
}