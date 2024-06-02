// <copyright file="IconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud;
using Dalamud.Interface.Internal;
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
    public IDalamudTextureWrap? GetIcon(uint iconId) => this.GetIcon(iconId, ITextureProvider.IconFlags.None);

    /// <inheritdoc/>
    public bool TryGetIcon(uint iconId, [NotNullWhen(true)] out IDalamudTextureWrap? textureWrap) => this.TryGetIcon(iconId, ITextureProvider.IconFlags.None, out textureWrap);

    /// <inheritdoc/>
    public IDalamudTextureWrap? GetIcon(uint iconId, ITextureProvider.IconFlags flags) => this.textureProvider.GetIcon(iconId, flags);

    /// <inheritdoc/>
    public bool TryGetIcon(uint iconId, ITextureProvider.IconFlags flags, [NotNullWhen(true)] out IDalamudTextureWrap? textureWrap)
    {
        textureWrap = this.GetIcon(iconId, flags);
        return textureWrap is not null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}