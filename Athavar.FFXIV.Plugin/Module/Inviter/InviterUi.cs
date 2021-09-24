using System;
using System.Linq;
using Athavar.FFXIV.Plugin;
using Dalamud.Game.Text;
using ImGuiNET;

namespace Inviter
{
    internal class InviterUi : IDisposable
    {
        private readonly InviterModule module;
        private Localizer Localizer => module.Localizer;

        public InviterUi(InviterModule module)
        {
            this.module = module;
        }

        public void Dispose()
        {
        }

        public void UiBuilder_OnBuildUi_ConfigTab()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Inviter"), ImGui.EndTabItem))
                return;


            if (ImGui.BeginChild("##SettingsRegion"))
            {
                if (ImGui.CollapsingHeader(Localizer.Localize("General Settings"), ImGuiTreeNodeFlags.DefaultOpen))
                    DrawGeneralSettings();
                if (ImGui.CollapsingHeader(Localizer.Localize("Filters")))
                    DrawFilters();
                ImGui.EndChild();
            }
        }

        private void DrawGeneralSettings()
        {
            var change = false;
            var config = module.InviterConfig;
            var toolTips = Modules.Configuration.ShowToolTips;
            if (ImGui.Checkbox(Localizer.Localize("Enable"), ref config.Enable))
            {
                change = true;
            }

            if (toolTips && ImGui.IsItemHovered())
                ImGui.SetTooltip(Localizer.Localize("Automatically invite people to your party (doesn't work for CWLS)."));
            
            ImGui.TextUnformatted(Localizer.Localize("Pattern:"));
            if (toolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Localizer.Localize("Pattern of the chat message to trigger the invitation."));
            }

            if (ImGui.InputText("##textPattern", ref config.TextPattern, 256))
            {
                this.module.invitePattern = null;
                change = true;
            }

            ImGui.SameLine(ImGui.GetColumnWidth() - 120);
            ImGui.TextUnformatted(Localizer.Localize("Regex"));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            if (ImGui.Checkbox("##regexMatch", ref config.RegexMatch)) change = true;
            if (toolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Localizer.Localize("Use regex to match the pattern to chat messages."));
            }

            ImGui.TextUnformatted(Localizer.Localize("Delay(ms):"));
            if (toolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Localizer.Localize("Delay the invitation after triggered."));
            }
            ImGui.SetNextItemWidth(150);
            if (ImGui.InputInt("##Delay", ref config.Delay, 10, 100)) change = true;

            if (ImGui.Checkbox(Localizer.Localize("Print Debug Message"), ref config.PrintMessage)) change = true;
            if (ImGui.Checkbox(Localizer.Localize("Print Error Message"), ref config.PrintError)) change = true;

            if (change)
            {
                Configuration.Save();
            }
        }

        private void DrawFilters()
        {
            ImGui.Columns(4, "FiltersTable", true);
            var config = module.InviterConfig;
            foreach (XivChatType chatType in Enum.GetValues<XivChatType>())
            {
                if (config.HiddenChatType.IndexOf(chatType) != -1) continue;
                var chatTypeName = Enum.GetName(chatType);
                bool checkboxClicked = config.FilteredChannels.IndexOf(chatType) == -1;
                if (ImGui.Checkbox(Localizer.Localize(chatTypeName) + "##filter", ref checkboxClicked))
                {
                    config.FilteredChannels = config.FilteredChannels.Distinct().ToList();
                    if (checkboxClicked)
                    {
                        if (config.FilteredChannels.IndexOf(chatType) != -1)
                            config.FilteredChannels.Remove(chatType);
                    }
                    else if (config.FilteredChannels.IndexOf(chatType) == -1)
                    {
                        config.FilteredChannels.Add(chatType);
                    }

                    config.FilteredChannels = config.FilteredChannels.Distinct().ToList();
                    config.FilteredChannels.Sort();
                    Configuration.Save();
                }

                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }
    }
}