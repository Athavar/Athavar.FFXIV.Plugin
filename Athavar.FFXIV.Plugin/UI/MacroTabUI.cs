using Dalamud.Game.ClientState.Keys;
using ImGuiNET;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Athavar.FFXIV.Plugin.UI
{
    public class MacroTabUI
    {
        private delegate ref int GetRefValueDelegate(int virtualKey);
        private GetRefValueDelegate? GetRefValue = null;

        private string? error;

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
                error = e.ToString();
            }

        }

        private string data = string.Empty;

        public void Draw()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Macro"), ImGui.EndTabItem))
                return;

            if (!string.IsNullOrEmpty(error))
            {
                ImGui.Text(error);
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
                    error = e.ToString();
                }
            }

            if (!string.IsNullOrEmpty(data))
            {
                ImGui.Text($"Data: {data}");
            }
        }

        private async Task PressKey(int millisecondsDuration, params VirtualKey[] virtualKey)
        {
            foreach(var key in virtualKey)
            {
                DownKey(key);
            }

            await Task.Delay(millisecondsDuration);

            foreach (var key in virtualKey)
            {
                UpKey(key);
            }
        }

        private async Task PressKey(int millisecondsDuration, VirtualKey virtualKey)
        {
            DownKey(virtualKey);
            await Task.Delay(millisecondsDuration);
            UpKey(virtualKey);
        }

        private void DownKey(VirtualKey virtualKey) => this.SetKey(virtualKey, true);

        private void UpKey(VirtualKey virtualKey) => this.SetKey(virtualKey, false);

        private unsafe void SetKey(VirtualKey virtualKey, bool value)
        {
            if (GetRefValue is null)
            {
                return;
            }

            GetRefValue.Invoke((int)virtualKey) = value ? 1 : 0;
        }

    }
}
