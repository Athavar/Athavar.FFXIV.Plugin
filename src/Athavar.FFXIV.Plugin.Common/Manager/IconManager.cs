// <copyright file="IconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;

/// <summary>
///     Cache for icons.
/// </summary>
internal sealed partial class IconManager : IIconManager, IDisposable
{
    private readonly IDataManager dataManager;
    private readonly ITextureProvider textureProvider;

    private readonly ClientLanguage language;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconManager"/> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    public IconManager(IDalamudServices dalamudServices)
    {
        this.dataManager = dalamudServices.DataManager;
        this.textureProvider = dalamudServices.TextureProvider;

        this.language = dalamudServices.ClientState.ClientLanguage;
    }

    /// <inheritdoc/>
    public ISharedImmediateTexture? GetIcon(uint iconId) => this.GetIcon(new GameIconLookup(iconId));

    /// <inheritdoc/>
    public bool TryGetIcon(uint iconId, [NotNullWhen(true)] out ISharedImmediateTexture? textureWrap) => this.TryGetIcon(new GameIconLookup(iconId), out textureWrap);

    /// <inheritdoc/>
    public ISharedImmediateTexture? GetIcon(GameIconLookup gameIconLookup) => this.textureProvider.GetFromGameIcon(gameIconLookup);

    /// <inheritdoc/>
    public bool TryGetIcon(GameIconLookup gameIconLookup, [NotNullWhen(true)] out ISharedImmediateTexture? textureWrap)
    {
        textureWrap = this.GetIcon(gameIconLookup);
        return textureWrap is not null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}