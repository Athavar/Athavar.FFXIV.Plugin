// <copyright file="IconManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud;
using Dalamud.Data;
using ImGuiScene;
using Lumina.Data.Files;

/// <summary>
///     Cache for icons.
/// </summary>
internal sealed partial class IconManager : IIconManager, IDisposable
{
    // ReSharper disable once CollectionNeverUpdated.Local; Data is updated on request.
    private readonly IconDataCache iconDataCache;

    private readonly DataManager dataManager;
    private readonly IIpcManager ipcManager;

    private readonly ClientLanguage language;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IconManager" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="ipcManager"><see cref="IIpcManager" /> added by DI.</param>
    public IconManager(IDalamudServices dalamudServices, IIpcManager ipcManager)
    {
        this.ipcManager = ipcManager;
        this.dataManager = dalamudServices.DataManager;

        this.language = dalamudServices.ClientState.ClientLanguage;

        this.iconDataCache = new IconDataCache(this);

        ipcManager.PenumbraStatusChanged += this.OnPenumbraStatusChanged;
    }

    /// <inheritdoc />
    public TextureWrap? GetIcon(uint iconId)
    {
        var path = GetIconPath(iconId, this.language);
        return this.iconDataCache[path];
    }

    /// <inheritdoc />
    public bool TryGetIcon(uint iconId, [NotNullWhen(true)] out TextureWrap? textureWrap)
    {
        textureWrap = this.GetIcon(iconId);
        return textureWrap is not null;
    }

    /// <inheritdoc />
    public TextureWrap? GetIcon(uint iconId, bool hr)
    {
        var path = GetIconPath(iconId, string.Empty, hr);
        return this.iconDataCache[path];
    }

    /// <inheritdoc />
    public bool TryGetIcon(uint iconId, bool hr, [NotNullWhen(true)] out TextureWrap? textureWrap)
    {
        textureWrap = this.GetIcon(iconId, hr);
        return textureWrap is not null;
    }

    /// <inheritdoc />
    public TextureWrap? GetHqIcon(uint iconId, bool isHq)
    {
        var path = GetIconPath(iconId, isHq ? "hq/" : string.Empty);
        return this.iconDataCache[path];
    }

    /// <inheritdoc />
    public bool TryHqGetIcon(uint iconId, bool isHq, [NotNullWhen(true)] out TextureWrap? textureWrap)
    {
        textureWrap = this.GetHqIcon(iconId, isHq);
        return textureWrap is not null;
    }

    /// <inheritdoc />
    public void CleanIconData() => this.iconDataCache.Clear();

    /// <inheritdoc />
    public void Dispose()
    {
        this.ipcManager.PenumbraStatusChanged -= this.OnPenumbraStatusChanged;
        this.CleanIconData();
    }

    /// <summary>
    ///     Gets the file path of an icon.
    /// </summary>
    /// <param name="iconId">The icon id.</param>
    /// <param name="iconLanguage">The game language.</param>
    /// <returns>returns the icon file path.</returns>
    private static string GetIconPath(uint iconId, ClientLanguage iconLanguage)
    {
        var languagePath = iconLanguage switch
                           {
                               ClientLanguage.Japanese => "ja/",
                               ClientLanguage.English => "en/",
                               ClientLanguage.German => "de/",
                               ClientLanguage.French => "fr/",
                               _ => "en/",
                           };

        return GetIconPath(iconId, languagePath);
    }

    /// <summary>
    ///     Gets the file path of an icon.
    /// </summary>
    /// <param name="iconId">The icon id.</param>
    /// <param name="type">The type of the icon (e.g. 'hq' to get the HQ variant of an item icon).</param>
    /// <param name="hr">if the Icon should hat a high resolution.</param>
    /// <returns>returns the icon file path.</returns>
    private static string GetIconPath(uint iconId, string type, bool hr = false)
    {
        if (type.Length > 0 && !type.EndsWith("/"))
        {
            type += "/";
        }

        var path = $"ui/icon/{iconId / 1000U:D3}000/{type}{iconId:D6}{(hr ? "_hr1" : string.Empty)}.tex";

        return path;
    }

    private void OnPenumbraStatusChanged(object? sender, EventArgs e) => this.iconDataCache.Clear();

    private TextureWrap? ResolveTexture(string path)
    {
        if (!this.dataManager.FileExists(path))
        {
            return null;
        }

        TexFile? tex = null;

        path = this.ipcManager.ResolvePenumbraPath(path);

        try
        {
            if (path[0] == '/' || path[1] == ':')
            {
                tex = this.dataManager.GameData.GetFileFromDisk<TexFile>(path);
            }
        }
        catch
        {
            // ignored
        }

        tex ??= this.dataManager.GetFile<TexFile>(path);
        return this.dataManager.GetImGuiTexture(tex);
    }

    private class IconDataCache : ConcurrentDictionary<string, TextureWrap?>, IDisposable
    {
        private readonly IconManager manager;
        private readonly object lockObject = new();

        public IconDataCache(IconManager manager) => this.manager = manager;

        public new TextureWrap? this[string path]
        {
            get
            {
                lock (this.lockObject)
                {
                    if (this.TryGetValue(path, out var iconData))
                    {
                        return iconData;
                    }

                    var imageData = this.manager.ResolveTexture(path);
                    this.TryAdd(path, imageData);
                    return imageData;
                }
            }
        }

        public void Dispose()
        {
            lock (this.lockObject)
            {
                foreach (var textureWrap in this.Values)
                {
                    textureWrap?.Dispose();
                }

                this.Clear();
            }
        }
    }
}