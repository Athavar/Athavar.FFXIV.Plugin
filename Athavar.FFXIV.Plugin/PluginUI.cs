using ImGuiNET;
using System;
using System.Numerics;

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
            localizer = Modules.Instance.Localizer;

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

            UiBuilder_OnBuildUi_SettingTab();

            Modules.Instance.Draw();

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
            var config = Modules.Instance.Configuration;

            // ToolTip setting
            var value = config.ShowToolTips;
            ImGui.TextUnformatted(localizer.Localize("Tooltips"));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            if (ImGui.Checkbox("##hideTooltipsOnOff", ref value))
            {
                config.ShowToolTips = value;
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

            ImGui.TextUnformatted(localizer.Localize("Modules:"));
            foreach (var module in Modules.ModuleValues)
            {
                var val = config.ModuleEnabled.Contains(module);
                if (ImGui.Checkbox(module.ToString(), ref val))
                {
                    Modules.Instance.Enable(module, val);
                    change = true;
                }
            }

            if (change)
            {
                Configuration.Save();
            }
        }
    }
}
