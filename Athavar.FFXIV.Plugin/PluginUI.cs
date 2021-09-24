using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Athavar.FFXIV.Plugin
{
    internal class PluginUI : IDisposable
    {
        private Plugin plugin;

        private Localizer localizer;

        private string[] languages = Enum.GetNames<Language>();

        public PluginUI(Plugin plugin)
        {
            this.plugin = plugin;
            localizer = Modules.Localizer;

            DalamudBinding.PluginInterface.UiBuilder.OpenConfigUi += UiBuilder_OnOpenConfigUi;
            DalamudBinding.PluginInterface.UiBuilder.Draw += UiBuilder_OnBuildUi_Config;
        }

        public void Dispose()
        {
            DalamudBinding.PluginInterface.UiBuilder.OpenConfigUi -= UiBuilder_OnOpenConfigUi;
            DalamudBinding.PluginInterface.UiBuilder.Draw -= UiBuilder_OnBuildUi_Config;
        }


#if DEBUG
        private bool IsImguiConfigOpen = true;
#else
        private bool IsImguiConfigOpen = false;
#endif

        public void OpenConfig() => IsImguiConfigOpen = true;

        public void UiBuilder_OnOpenConfigUi() => IsImguiConfigOpen = true;

        public void UiBuilder_OnBuildUi_Config()
        {
            if (!IsImguiConfigOpen)
                return;

            ImGui.SetNextWindowSize(new Vector2(525, 600), ImGuiCond.FirstUseEver);

            ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);

            ImGui.Begin(plugin.Name, ref IsImguiConfigOpen);

            ImGui.BeginTabBar("##tabBar");

            Modules.MacroModule.Draw();
            Modules.YesModule.Draw();
            Modules.InviterModule.Draw();

            UiBuilder_OnBuildUi_SettingTab();

            ImGui.BeginTabItem("Test");

            var addon = DalamudBinding.GameGui.GetAddonByName("JournalDetail", 1);
            if (addon != IntPtr.Zero)
            {
                ImGui.Text("Ist da.");
            }

            ImGui.EndTabItem();

            ImGui.EndTabBar();
            ImGui.End();

            ImGui.PopStyleColor();
        }

        private void UiBuilder_OnBuildUi_SettingTab()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Settings"), ImGui.EndTabItem))
                return;

            var change = false;
            var config = Modules.Configuration;

            // ToolTip setting
            var value = config.ShowToolTips;
            ImGui.TextUnformatted(localizer.Localize("Tooltips"));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            if (ImGui.Checkbox("##hideTooltipsOnOff", ref value))
            {
                Modules.Configuration.ShowToolTips = value;
                change = true;
            }

            // Language setting
            var selectedLanguage = (int)config.Language;
            ImGui.TextUnformatted(localizer.Localize("Language:"));
            if (config.ShowToolTips && ImGui.IsItemHovered())
                ImGui.SetTooltip(localizer.Localize("Change the UI Language."));
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.Combo("##hideLangSetting", ref selectedLanguage, languages, languages.Length))
            {
                localizer.Language = config.Language = (Language)selectedLanguage;
                change = true;
            }

            if (change)
            {
                Configuration.Save();
            }
        }
    }
}
