// <copyright file="ImGuiEx.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Utils;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Athavar.FFXIV.Plugin.Common.Extension;
using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using ImGuiNET;

/// <summary>
///     ImGui wrappers.
/// </summary>
public static class ImGuiEx
{
    public static unsafe bool BeginTabItem(string label, ImGuiTabItemFlags flags)
    {
        var unterminatedLabelBytes = Encoding.UTF8.GetBytes(label);
        var labelBytes = stackalloc byte[unterminatedLabelBytes.Length + 1];
        fixed (byte* unterminatedPtr = unterminatedLabelBytes)
        {
            Buffer.MemoryCopy(unterminatedPtr, labelBytes, unterminatedLabelBytes.Length + 1, unterminatedLabelBytes.Length);
        }

        labelBytes[unterminatedLabelBytes.Length] = 0;

        var num2 = (int)ImGuiNative.igBeginTabItem(labelBytes, null, flags);
        return (uint)num2 > 0U;
    }

    /// <summary>
    ///     Create an icon.
    /// </summary>
    /// <param name="icon">Icon to display.</param>
    /// <param name="tooltip">Tooltip to display.</param>
    /// <param name="width">Width of the button.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Icon(FontAwesomeIcon icon, string? tooltip = null, int width = -1)
    {
        ImGui.PushFont(UiBuilder.IconFont);

        if (width > 0)
        {
            ImGui.SetNextItemWidth(width);
        }

        var label = $"{icon.ToIconString()}";
        ImGui.Text(label);
        ImGui.PopFont();

        if (tooltip != null)
        {
            TextTooltip(tooltip);
        }
    }

    /// <summary>
    ///     Create an icon button.
    /// </summary>
    /// <param name="icon">Icon to display.</param>
    /// <param name="tooltip">Tooltip to display.</param>
    /// <param name="width">Width of the button.</param>
    /// <param name="small">Use the small button.</param>
    /// <param name="disabled">Disable state of the button.</param>
    /// <returns>A value indicating whether the button has been pressed.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IconButton(FontAwesomeIcon icon, string? tooltip = null, int width = -1, bool small = false, bool disabled = false)
    {
        if (disabled)
        {
            ImGui.BeginDisabled();
        }

        ImGui.PushFont(UiBuilder.IconFont);

        if (width > 0)
        {
            ImGui.SetNextItemWidth(width);
        }

        var label = $"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}";
        var result = small ? ImGui.SmallButton(label) : ImGui.Button(label);
        ImGui.PopFont();

        if (disabled)
        {
            ImGui.EndDisabled();
        }

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetIconButtonWidth(FontAwesomeIcon icon)
    {
        var style = ImGui.GetStyle();

        return GetIconWidth(icon) + style.FramePadding.X * 2;
    }

    /// <summary>
    ///     Creates a simple text tooltip.
    /// </summary>
    /// <param name="text">Text to display.</param>
    /// <param name="flags">Hovered Flags.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TextTooltip(string text, ImGuiHoveredFlags flags = ImGuiHoveredFlags.None)
    {
        if (ImGui.IsItemHovered(flags))
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

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void Center(string text)
    {
        var offset = (ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.TextUnformatted(text);
    }

    /// <summary>
    ///     Scales a Image after height.
    /// </summary>
    /// <param name="dalamudTextureWrap">Image texture.</param>
    /// <param name="scaledHeight">Scaled height.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ScaledCenterImageY(IDalamudTextureWrap dalamudTextureWrap, float scaledHeight)
    {
        var num = scaledHeight / dalamudTextureWrap.Height;
        var x = dalamudTextureWrap.Width * num;
        var offset = (ImGui.GetContentRegionAvail().X - x) / 2;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        ImGui.Image(dalamudTextureWrap.ImGuiHandle, new Vector2(x, scaledHeight));
    }

    /// <summary>
    ///     Scales a Image after height.
    /// </summary>
    /// <param name="handle">Pointer to the image texture.</param>
    /// <param name="size">Image size.</param>
    /// <param name="scaledHeight">Scaled height.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ScaledImageY(nint handle, Vector2 size, float scaledHeight) => ScaledImageY(handle, (int)size.X, (int)size.Y, scaledHeight);

    /// <summary>
    ///     Scales a Image after height.
    /// </summary>
    /// <param name="textureWrap">The wrap image texture.</param>
    /// <param name="scaledHeight">Scaled height.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ScaledImageY(IDalamudTextureWrap? textureWrap, float scaledHeight)
    {
        if (textureWrap is not null)
        {
            ScaledImageY(textureWrap.ImGuiHandle, textureWrap.Width, textureWrap.Height, scaledHeight);
        }
    }

    /// <summary>
    ///     Scales a Image after height.
    /// </summary>
    /// <param name="handle">Pointer to the image texture.</param>
    /// <param name="iconWidth">Image width.</param>
    /// <param name="iconHeight">Image height.</param>
    /// <param name="scaledHeight">Scaled height.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ScaledImageY(nint handle, int iconWidth, int iconHeight, float scaledHeight)
    {
        var num = scaledHeight / iconHeight;
        var x = iconWidth * num;
        ImGui.Image(handle, new Vector2(x, scaledHeight));
    }

    /// <summary>
    ///     Image.
    /// </summary>
    /// <param name="textureWrap">The wrap image texture.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Image(IDalamudTextureWrap? textureWrap)
    {
        if (textureWrap is not null)
        {
            ImGui.Image(textureWrap.ImGuiHandle, new Vector2(textureWrap.Width, textureWrap.Height));
        }
    }

    /// <summary>
    ///     Color text based on the condition.
    /// </summary>
    /// <param name="condition">The condition.</param>
    /// <param name="color">The color.</param>
    /// <param name="text">The text.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TableRow(params object[] values)
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

    /// <summary>
    ///     Draw a table row based on the input values.
    /// </summary>
    /// <param name="values">The input values.</param>
    /// <param name="rowItemActionTrigger">The trigger for the action.</param>
    /// <param name="rowItemAction">The triggered action.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TableRow(object[] values, Func<bool> rowItemActionTrigger, Action<int> rowItemAction)
    {
        for (var columnIndex = 0; columnIndex < values.Length; columnIndex++)
        {
            ImGui.TableSetColumnIndex(columnIndex);
            switch (values[columnIndex])
            {
                case string text:
                    ImGui.TextUnformatted(text);
                    break;
                case Action action:
                    action();
                    break;
            }

            if (rowItemActionTrigger())
            {
                rowItemAction(columnIndex);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DragFloat2(string label, Vector2 value, Action<Vector2> setter, float vSpeed = 1f, float vMin = 0f, float vMax = 0f)
    {
        if (!ImGui.DragFloat2(label, ref value, vSpeed, vMin, vMax))
        {
            return false;
        }

        setter(value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Checkbox(string label, bool value, Action<bool> setter)
    {
        if (!ImGui.Checkbox(label, ref value))
        {
            return false;
        }

        setter(value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ColorEdit4(string label, Vector4 value, Action<Vector4> setter, ImGuiColorEditFlags alphaPreview)
    {
        if (!ImGui.ColorEdit4(label, ref value, alphaPreview))
        {
            return false;
        }

        setter(value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DragInt(string label, int value, Action<int> setter, int vSpeed, int vMin = 0, int vMax = 0)
    {
        if (!ImGui.DragInt(label, ref value, vSpeed, vMin, vMax))
        {
            return false;
        }

        setter(value);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DragIntRange2(string label, int vCurrentMin, Action<int> minSetter, int vCurrentMax, Action<int> maxSetter, int vSpeed, int vMin = 0, int vMax = 0, string format = "%d")
    {
        if (!ImGui.DragIntRange2(label, ref vCurrentMin, ref vCurrentMax, vSpeed, vMin, vMax, format))
        {
            return false;
        }

        minSetter(vCurrentMin);
        maxSetter(vCurrentMax);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Combo<T>(string label, T selectedItem, Action<T> setter, string[] items)
        where T : struct, Enum
        => Combo(label, selectedItem.AsText(), x => setter(Enum.Parse<T>(items[x])), items);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Combo(string label, string selectedItem, Action<int> setter, string[] items) => Combo(label, Array.IndexOf(items, selectedItem), setter, items);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Combo(string label, int selectedIndex, Action<int> setter, string[] items)
    {
        if (!ImGui.Combo(label, ref selectedIndex, items, items.Length))
        {
            return false;
        }

        setter(selectedIndex);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawNestIndicator(int depth)
    {
        // This draws the L shaped symbols and padding to the left of config items collapsible under a checkbox.
        // Shift cursor to the right to pad for children with depth more than 1.
        var oldCursor = ImGui.GetCursorPos();
        var offset = new Vector2(26 * Math.Max(depth - 1, 0), 2);
        ImGui.SetCursorPos(oldCursor + offset);
        ImGui.TextColored(new Vector4(229f / 255f, 57f / 255f, 57f / 255f, 1f), "\u2002\u2514");
        ImGui.SameLine();
        ImGui.SetCursorPosY(oldCursor.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Spacing(int spacingSize)
    {
        for (var i = 0; i < spacingSize; i++)
        {
            ImGui.NewLine();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InputText(string label, string input, Action<string> setter, uint maxLength)
    {
        if (!ImGui.InputText(label, ref input, maxLength))
        {
            return false;
        }

        setter(input);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawText(
        ImDrawListPtr drawList,
        string text,
        Vector2 pos,
        uint color,
        bool outline,
        uint outlineColor = 0xFF000000,
        int thickness = 1)
    {
        // outline
        if (outline)
        {
            for (var i = 1; i < thickness + 1; i++)
            {
                drawList.AddText(new Vector2(pos.X - i, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y + i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X - i, pos.Y), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y), outlineColor, text);
                drawList.AddText(new Vector2(pos.X - i, pos.Y - i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X, pos.Y - i), outlineColor, text);
                drawList.AddText(new Vector2(pos.X + i, pos.Y - i), outlineColor, text);
            }
        }

        // text
        drawList.AddText(new Vector2(pos.X, pos.Y), color, text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawInWindow(
        string name,
        Vector2 pos,
        Vector2 size,
        bool needsInput,
        bool setPosition,
        Action<ImDrawListPtr> drawAction)
        => DrawInWindow(name, pos, size, needsInput, false, setPosition, drawAction);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DrawInWindow(
        string name,
        Vector2 pos,
        Vector2 size,
        bool needsInput,
        bool needsFocus,
        bool locked,
        Action<ImDrawListPtr> drawAction,
        ImGuiWindowFlags extraFlags = ImGuiWindowFlags.None)
    {
        var windowFlags =
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.ChildWindow |
            extraFlags;

        if (!needsInput)
        {
            windowFlags |= ImGuiWindowFlags.NoInputs;
        }

        if (!needsFocus)
        {
            windowFlags |= ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoBringToFrontOnFocus;
        }

        if (locked)
        {
            windowFlags |= ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize;
            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowPos(pos);
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

        if (ImGui.Begin(name, windowFlags))
        {
            drawAction(ImGui.GetWindowDrawList());
        }

        ImGui.PopStyleVar(3);
        ImGui.EndChild();
    }
}