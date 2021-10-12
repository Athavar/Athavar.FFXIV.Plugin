namespace Athavar.FFXIV.Plugin
{
    using System;
    using System.Numerics;
    using Dalamud.Interface.Windowing;
    using ImGuiNET;

    internal class PluginWindow : Window
    {
        private Plugin plugin;

        private Localizer localizer;

        private string[] languages = Enum.GetNames<Language>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginWindow"/> class.
        /// </summary>
        /// <param name="plugin">The <see cref="Plugin"/>.</param>
        public PluginWindow(Plugin plugin)
            : base(plugin.Name)
        {
            this.plugin = plugin;
            this.localizer = Modules.Instance.Localizer;

            this.Size = new Vector2(525, 600);
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.RespectCloseHotkey = false;

#if DEBUG
            this.Toggle();
#endif
        }

        /// <inheritdoc/>
        public override void PreDraw()
        {
            ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
        }

        /// <inheritdoc/>
        public override void PostDraw()
        {
            ImGui.PopStyleColor();
        }

        /// <inheritdoc/>
        public override void Draw()
        {
            ImGui.BeginTabBar("##tabBar");

            this.DrawSettingTab();

            Modules.Instance.Draw();

            ImGui.EndTabBar();
            ImGui.End();
        }

        private void DrawSettingTab()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Settings"), ImGui.EndTabItem))
            {
                return;
            }

            var change = false;
            var config = Modules.Instance.Configuration;

            // ToolTip setting
            var value = config.ShowToolTips;
            ImGui.TextUnformatted(this.localizer.Localize("Tooltips"));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            if (ImGui.Checkbox("##hideTooltipsOnOff", ref value))
            {
                config.ShowToolTips = value;
                change = true;
            }

            // Language setting
            var selectedLanguage = (int)config.Language;
            ImGui.TextUnformatted(this.localizer.Localize("Language:"));
            if (config.ShowToolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(this.localizer.Localize("Change the UI Language."));
            }

            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.Combo("##hideLangSetting", ref selectedLanguage, this.languages, this.languages.Length))
            {
                this.localizer.Language = config.Language = (Language)selectedLanguage;
                change = true;
            }

            ImGui.TextUnformatted(this.localizer.Localize("Modules:"));
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
