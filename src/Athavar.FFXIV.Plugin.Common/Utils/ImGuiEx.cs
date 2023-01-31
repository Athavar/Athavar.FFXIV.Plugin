// <copyright file="ImGuiEx.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Utils;

using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;

/// <summary>
///     ImGui wrappers.
/// </summary>
public static class ImGuiEx
{
    /// <summary>
    ///     Create an icon button.
    /// </summary>
    /// <param name="icon">Icon to display.</param>
    /// <param name="tooltip">Tooltip to display.</param>
    /// <param name="width">Width of the button.</param>
    /// <param name="small">Use the small button.</param>
    /// <returns>A value indicating whether the button has been pressed.</returns>
    public static bool IconButton(FontAwesomeIcon icon, string? tooltip = null, int width = -1, bool small = false)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        if (width > 0)
        {
            ImGui.SetNextItemWidth(32);
        }

        var label = $"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}";
        var result = small ? ImGui.SmallButton(label) : ImGui.Button(label);
        ImGui.PopFont();

        if (tooltip != null)
        {
            TextTooltip(tooltip);
        }

        return result;
    }

    /// <summary>
    ///     Gets the width of an icon.
    /// </summary>
    /// <param name="icon">Icon to measure.</param>
    /// <returns>The size of the icon.</returns>
    public static float GetIconWidth(FontAwesomeIcon icon)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        var width = ImGui.CalcTextSize($"{icon.ToIconString()}").X;

        ImGui.PopFont();

        return width;
    }

    /// <summary>
    ///     Gets the  width of an icon button.
    /// </summary>
    /// <param name="icon">Icon to measure.</param>
    /// <returns>The size of the icon.</returns>
    public static float GetIconButtonWidth(FontAwesomeIcon icon)
    {
        var style = ImGui.GetStyle();

        return GetIconWidth(icon) + (style.FramePadding.X * 2);
    }

    /// <summary>
    ///     Creates a simple text tooltip.
    /// </summary>
    /// <param name="text">Text to display.</param>
    public static void TextTooltip(string text)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(text);
            ImGui.EndTooltip();
        }
    }

    /// <summary>
    ///     Get the current RGBA color for the given widget.
    /// </summary>
    /// <param name="col">The type of color to fetch.</param>
    /// <returns>A RGBA vec4.</returns>
    public static Vector4 GetStyleColorVec4(ImGuiCol col)
    {
        unsafe
        {
            return *ImGui.GetStyleColorVec4(ImGuiCol.Button);
        }
    }

    /// <summary>
    ///     Get a list clipper.
    /// </summary>
    /// <param name="itemsCount">Amount of items in the list.</param>
    /// <returns>A RGBA vec4.</returns>
    public static ImGuiListClipperPtr Clipper(int itemsCount)
    {
        unsafe
        {
            var guiListClipperPtr = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
            guiListClipperPtr.Begin(itemsCount);
            return guiListClipperPtr;
        }
    }

    /// <summary>
    ///     Scales a Image after height.
    /// </summary>
    /// <param name="handle">Pointer to the image texture.</param>
    /// <param name="iconWidth">Image width.</param>
    /// <param name="iconHeight">Image height.</param>
    /// <param name="scaledHeight">Scaled height.</param>
    public static void ScaledImageY(nint handle, int iconWidth, int iconHeight, float scaledHeight)
    {
        var num = scaledHeight / iconHeight;
        var x = iconWidth * num;
        ImGui.Image(handle, new Vector2(x, scaledHeight));
    }

    /// <summary>
    ///     Color text based on the condition.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="color">The color.</param>
    /// <param name="text">The text.</param>
    public static void TextColorCondition(bool condition, Vector4 color, string text)
    {
        if (condition)
        {
            ImGui.TextColored(color, text);
        }
        else
        {
            ImGui.TextUnformatted(text);
        }
    }

    /// <summary>
    ///     Draw a table row based on the input values.
    /// </summary>
    /// <param name="values">The input values.</param>
    public static void TableRow(object[] values)
    {
        for (var columnIndex = 0; columnIndex < values.Length; ++columnIndex)
        {
            ImGui.TableSetColumnIndex(columnIndex);
            switch (values[columnIndex])
            {
                case string text:
                    ImGui.TextUnformatted(text);
                    continue;
                case Action action:
                    action();
                    continue;
                case null:
                    continue;
                default:
                    ImGui.TextUnformatted(values[columnIndex].ToString());
                    continue;
            }
        }
    }
}