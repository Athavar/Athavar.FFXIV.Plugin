// <copyright file="FontsManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Reflection;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using ImGuiNET;

public class FontsManager : IDisposable, IFontsManager
{
    public static readonly List<string> DefaultFontKeys = new() { "Expressway_24", "Expressway_20", "Expressway_16" };

    private static FontsManager? instance;

    private readonly IPluginLogger logger;
    private readonly Dictionary<string, IFontHandle> imGuiFonts = [];
    private readonly string userFontPath = string.Empty;
    private readonly UiBuilder uiBuilder;
    private string[] fontList;

    public FontsManager(IDalamudServices services, IEnumerable<IFontsManager.FontData> fonts)
    {
        this.logger = services.PluginLogger;
        instance = this;
        this.fontList = new[] { Constants.FontsManager.DalamudFontKey };

        this.uiBuilder = services.PluginInterface.UiBuilder;
        /* uiBuilder.RebuildFonts(); */
        this.BuildFonts(fonts);
        this.userFontPath = $"{services.PluginInterface.GetPluginConfigDirectory()}\\Fonts\\";
    }

    public static string DefaultBigFontKey => DefaultFontKeys[0];

    public static string DefaultMediumFontKey => DefaultFontKeys[1];

    public static string DefaultSmallFontKey => DefaultFontKeys[2];

    public static bool ValidateFont(string[] fontOptions, int fontId, string fontKey) => fontId < fontOptions.Length && fontOptions[fontId].Equals(fontKey);

    public static FontScope? PushFont(string fontKey)
    {
        var manager = instance;
        if (instance is null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(fontKey) ||
            !manager!.imGuiFonts.TryGetValue(fontKey, out var fontHandler))
        {
            return new(null);
        }

        return new(fontHandler);
    }

    public static string[] GetFontList() => instance?.fontList ?? [];

    public static string GetFontKey(IFontsManager.FontData font)
    {
        var key = $"{font.Name}_{font.Size}";
        key += font.Chinese ? "_cnjp" : string.Empty;
        key += font.Korean ? "_kr" : string.Empty;
        return key;
    }

    public static string GetPluginFontPath()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (path is not null)
        {
            return $"{path}\\Media\\Fonts\\";
        }

        return string.Empty;
    }

    public static string[] GetFontNamesFromPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return [];
        }

        string[] fonts;
        try
        {
            fonts = Directory.GetFiles(path, "*.ttf");
        }
        catch
        {
            fonts = [];
        }

        return fonts
           .Select(
                f => f
                   .Replace(path, string.Empty)
                   .Replace(".ttf", string.Empty, StringComparison.OrdinalIgnoreCase))
           .ToArray();
    }

    public string GetUserFontPath() => this.userFontPath;

    public void CopyPluginFontsToUserPath()
    {
        var pluginFontPath = GetPluginFontPath();
        var userFontPath = this.GetUserFontPath();

        if (string.IsNullOrEmpty(pluginFontPath) || string.IsNullOrEmpty(userFontPath))
        {
            return;
        }

        if (!Directory.Exists(userFontPath))
        {
            try
            {
                Directory.CreateDirectory(userFontPath);
            }
            catch (Exception ex)
            {
                this.logger.Warning($"Failed to create User Font Directory {ex}");
            }
        }

        if (!Directory.Exists(userFontPath))
        {
            return;
        }

        string[] pluginFonts;
        try
        {
            pluginFonts = Directory.GetFiles(pluginFontPath, "*.ttf");
        }
        catch
        {
            pluginFonts = [];
        }

        foreach (var font in pluginFonts)
        {
            try
            {
                if (!string.IsNullOrEmpty(font))
                {
                    var fileName = font.Replace(pluginFontPath, string.Empty);
                    var copyPath = Path.Combine(userFontPath, fileName);
                    if (!File.Exists(copyPath))
                    {
                        File.Copy(font, copyPath, false);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.Warning($"Failed to copy font {font} to User Font Directory: {ex}");
            }
        }
    }

    public int GetFontIndex(string fontKey)
    {
        for (var i = 0; i < this.fontList.Length; i++)
        {
            if (this.fontList[i].Equals(fontKey))
            {
                return i;
            }
        }

        return 0;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.DisposeFontHandles();
        }
    }

    private void DisposeFontHandles()
    {
        foreach (var (_, value) in this.imGuiFonts)
        {
            value.Dispose();
        }

        this.imGuiFonts.Clear();
    }

    private void BuildFonts(IEnumerable<IFontsManager.FontData> fontData)
    {
        var fontDir = this.GetUserFontPath();

        if (string.IsNullOrEmpty(fontDir))
        {
            return;
        }

        this.DisposeFontHandles();
        var io = ImGui.GetIO();

        foreach (var font in fontData)
        {
            var fontPath = $"{fontDir}{font.Name}.ttf";
            if (!File.Exists(fontPath))
            {
                continue;
            }

            try
            {
                var imFont = this.uiBuilder.FontAtlas.NewDelegateFontHandle(
                    e => e.OnPreBuild(
                        tk => tk.AddFontFromFile(
                            fontPath,
                            new()
                            {
                                SizePx = font.Size,
                                GlyphRanges = this.GetCharacterRanges(font, io),
                            })));

                this.imGuiFonts.Add(GetFontKey(font), imFont);
            }
            catch (Exception ex)
            {
                this.logger.Error($"Failed to load font from path [{fontPath}]!");
                this.logger.Error(ex.ToString());
            }
        }

        var fontList = new List<string> { Constants.FontsManager.DalamudFontKey };
        fontList.AddRange(this.imGuiFonts.Keys);
        this.fontList = fontList.ToArray();
    }

    private unsafe ushort[]? GetCharacterRanges(IFontsManager.FontData font, ImGuiIOPtr io)
    {
        if (font is { Chinese: false, Korean: false })
        {
            return null;
        }

        var builder = new ImFontGlyphRangesBuilderPtr(ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder());

        if (font.Chinese)
        {
            // GetGlyphRangesChineseFull() includes Default + Hiragana, Katakana, Half-Width, Selection of 1946 Ideographs
            // https://skia.googlesource.com/external/github.com/ocornut/imgui/+/v1.53/extra_fonts/README.txt
            builder.AddRanges(io.Fonts.GetGlyphRangesChineseFull());
        }

        if (font.Korean)
        {
            builder.AddRanges(io.Fonts.GetGlyphRangesKorean());
        }

        return builder.BuildRangesToArray();
    }

    public sealed class FontScope : IDisposable
    {
        private readonly IFontHandle? handle;

        internal FontScope(IFontHandle? handle)
        {
            this.handle = handle;
            this.handle?.Push();
        }

        public void Dispose() => this.handle?.Pop();
    }
}