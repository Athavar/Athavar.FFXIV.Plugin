// <copyright file="FontsManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Reflection;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;

public class FontsManager : IDisposable, IFontsManager
{
    public static readonly List<string> DefaultFontKeys = new() { "Expressway_24", "Expressway_20", "Expressway_16" };

    private static FontsManager? instance;
    private static UiBuilder? uiBuilder;
    private static string userFontPath = string.Empty;
    private readonly Dictionary<string, ImFontPtr> imGuiFonts;
    private IEnumerable<IFontsManager.FontData> fontData;
    private string[] fontList;

    public FontsManager(IDalamudServices services, IEnumerable<IFontsManager.FontData> fonts)
    {
        instance = this;
        this.fontData = fonts;
        this.fontList = new[] { Constants.FontsManager.DalamudFontKey };
        this.imGuiFonts = new Dictionary<string, ImFontPtr>();

        uiBuilder = services.PluginInterface.UiBuilder;
        uiBuilder.BuildFonts += this.BuildFonts;
        uiBuilder.RebuildFonts();
        userFontPath = $"{services.PluginInterface.GetPluginConfigDirectory()}\\Fonts\\";
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
            fontKey.Equals(Constants.FontsManager.DalamudFontKey) ||
            !manager!.imGuiFonts.Keys.Contains(fontKey))
        {
            return new FontScope(false);
        }

        ImGui.PushFont(manager.imGuiFonts[fontKey]);
        return new FontScope(true);
    }

    public static string[] GetFontList() => instance?.fontList ?? Array.Empty<string>();

    public static string GetFontKey(IFontsManager.FontData font)
    {
        var key = $"{font.Name}_{font.Size}";
        key += font.Chinese ? "_cnjp" : string.Empty;
        key += font.Korean ? "_kr" : string.Empty;
        return key;
    }

    public static void CopyPluginFontsToUserPath()
    {
        var pluginFontPath = GetPluginFontPath();
        var userFontPath = GetUserFontPath();

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
                PluginLog.Warning($"Failed to create User Font Directory {ex}");
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
            pluginFonts = new string[0];
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
                PluginLog.Warning($"Failed to copy font {font} to User Font Directory: {ex}");
            }
        }
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

    public static string GetUserFontPath() => userFontPath;

    public static string[] GetFontNamesFromPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return new string[0];
        }

        string[] fonts;
        try
        {
            fonts = Directory.GetFiles(path, "*.ttf");
        }
        catch
        {
            fonts = new string[0];
        }

        return fonts
           .Select(f => f
               .Replace(path, string.Empty)
               .Replace(".ttf", string.Empty, StringComparison.OrdinalIgnoreCase))
           .ToArray();
    }

    public void BuildFonts()
    {
        var fontDir = GetUserFontPath();

        if (string.IsNullOrEmpty(fontDir))
        {
            return;
        }

        this.imGuiFonts.Clear();
        var io = ImGui.GetIO();

        foreach (var font in this.fontData)
        {
            var fontPath = $"{fontDir}{font.Name}.ttf";
            if (!File.Exists(fontPath))
            {
                continue;
            }

            try
            {
                var ranges = this.GetCharacterRanges(font, io);

                var imFont = !ranges.HasValue
                    ? io.Fonts.AddFontFromFileTTF(fontPath, font.Size)
                    : io.Fonts.AddFontFromFileTTF(fontPath, font.Size, null, ranges.Value.Data);

                this.imGuiFonts.Add(GetFontKey(font), imFont);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to load font from path [{fontPath}]!");
                PluginLog.Error(ex.ToString());
            }
        }

        var fontList = new List<string> { Constants.FontsManager.DalamudFontKey };
        fontList.AddRange(this.imGuiFonts.Keys);
        this.fontList = fontList.ToArray();
    }

    public void UpdateFonts(IEnumerable<IFontsManager.FontData> fonts)
    {
        this.fontData = fonts;
        uiBuilder?.RebuildFonts();
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
        instance = null;
        uiBuilder = null;
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (uiBuilder != null)
            {
                uiBuilder.BuildFonts -= this.BuildFonts;
            }

            this.imGuiFonts.Clear();
        }
    }

    private unsafe ImVector? GetCharacterRanges(IFontsManager.FontData font, ImGuiIOPtr io)
    {
        if (!font.Chinese && !font.Korean)
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

        builder.BuildRanges(out var ranges);

        return ranges;
    }

    public class FontScope : IDisposable
    {
        private readonly bool fontPushed;

        public FontScope(bool fontPushed) => this.fontPushed = fontPushed;

        public void Dispose()
        {
            if (this.fontPushed)
            {
                ImGui.PopFont();
            }
        }
    }
}