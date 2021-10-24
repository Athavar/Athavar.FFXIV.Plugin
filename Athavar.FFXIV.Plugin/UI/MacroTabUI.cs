// <copyright file="MacroTabUI.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.UI
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;

    using Dalamud.Game.ClientState.Keys;
    using ImGuiNET;

    public class MacroTabUI
    {
        private delegate ref int GetRefValueDelegate(int virtualKey);

        private GetRefValueDelegate? GetRefValue = null;

        private string? error;

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroTabUI"/> class.
        /// </summary>
        public MacroTabUI()
        {
            try
            {
                var getRefValue = typeof(KeyState).GetMethod("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new NotImplementedException();
                var localgetRefValue = typeof(MacroTabUI).GetField("GetRefValue", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new NotImplementedException();
                var del = Delegate.CreateDelegate(localgetRefValue.FieldType, DalamudBinding.KeyState, getRefValue);
                localgetRefValue.SetValue(this, del);
            }
            catch (Exception e)
            {
                this.error = e.ToString();
            }

        }

        private string data = string.Empty;

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
                DalamudBinding.KeyState.ClearAll();
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
            foreach(var key in virtualKey)
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

        private unsafe void SetKey(VirtualKey virtualKey, bool value)
        {
            if (this.GetRefValue is null)
            {
                return;
            }

            this.GetRefValue.Invoke((int)virtualKey) = value ? 1 : 0;
        }

    }
}
