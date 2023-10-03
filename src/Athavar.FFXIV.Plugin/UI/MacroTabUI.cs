// <copyright file="MacroTabUI.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.UI;

using System.Reflection;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Dalamud.Game.ClientState.Keys;
using ImGuiNET;

internal class MacroTabUi
{
    private readonly IDalamudServices dalamudServices;
    private readonly GetRefValueDelegate? getRefValue = null;

    private readonly string data = string.Empty;

    private string? error;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroTabUi" /> class.
    /// </summary>
    public MacroTabUi(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;
        try
        {
            var type = Type.GetType("Dalamud.Game.ClientState.Keys.KeyState") ?? throw new AthavarPluginException("Fail to get KeyState");
            var getRefValue = type.GetMethod("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Method GetRefValue not found in class KeyState");
            var localgetRefValue = typeof(MacroTabUi).GetField("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Method GetRefValue not found in class MacroTabUi");
            var del = Delegate.CreateDelegate(localgetRefValue.FieldType, dalamudServices.KeyState, getRefValue);
            localgetRefValue.SetValue(this, del);
        }
        catch (Exception e)
        {
            this.error = e.ToString();
        }
    }

    private delegate ref int GetRefValueDelegate(int virtualKey);

    public void Draw()
    {
        using var raii = new ImGuiRaii();
        if (!raii.Begin(() => ImGui.BeginTabItem("Macro"), ImGui.EndTabItem))
        {
            return;
        }

        if (!string.IsNullOrEmpty(this.error))
        {
            ImGui.Text(this.error);
            return;
        }

        if (ImGui.Button("Clear"))
        {
            this.dalamudServices.KeyState.ClearAll();
        }

        if (ImGui.Button("A"))
        {
            try
            {
                Task.Run(() => this.PressKey(100, VirtualKey.SHIFT, VirtualKey.NUMPAD4));
            }
            catch (Exception e)
            {
                this.error = e.ToString();
            }
        }

        if (!string.IsNullOrEmpty(this.data))
        {
            ImGui.Text($"Data: {this.data}");
        }
    }

    private async Task PressKey(int millisecondsDuration, params VirtualKey[] virtualKey)
    {
        foreach (var key in virtualKey)
        {
            this.DownKey(key);
        }

        await Task.Delay(millisecondsDuration);

        foreach (var key in virtualKey)
        {
            this.UpKey(key);
        }
    }

    private async Task PressKey(int millisecondsDuration, VirtualKey virtualKey)
    {
        this.DownKey(virtualKey);
        await Task.Delay(millisecondsDuration);
        this.UpKey(virtualKey);
    }

    private void DownKey(VirtualKey virtualKey) => this.SetKey(virtualKey, true);

    private void UpKey(VirtualKey virtualKey) => this.SetKey(virtualKey, false);

    private void SetKey(VirtualKey virtualKey, bool value)
    {
        if (this.getRefValue is null)
        {
            return;
        }

        this.getRefValue.Invoke((int)virtualKey) = value ? 1 : 0;
    }
}