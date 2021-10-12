namespace Inviter
{
    using System;
    using System.Linq;
    using Athavar.FFXIV.Plugin;
    using Dalamud.Game.Text;
    using ImGuiNET;

    /// <summary>
    /// The UI of the inviter module.
    /// </summary>
    internal class InviterUi
    {
        private readonly InviterModule module;

        /// <summary>
        /// Initializes a new instance of the <see cref="InviterUi"/> class.
        /// </summary>
        /// <param name="module"></param>
        public InviterUi(InviterModule module)
        {
            this.module = module;
        }

        private Localizer Localizer => this.module.Localizer;

        public void DrawTab()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Inviter"), ImGui.EndTabItem))
            {
                return;
            }

            if (ImGui.BeginChild("##SettingsRegion"))
            {
                if (ImGui.CollapsingHeader(this.Localizer.Localize("General Settings"), ImGuiTreeNodeFlags.DefaultOpen))
                {
                    this.DrawGeneralSettings();
                }

                if (ImGui.CollapsingHeader(this.Localizer.Localize("Filters")))
                {
                    this.DrawFilters();
                }

                ImGui.EndChild();
            }
        }

        private void DrawGeneralSettings()
        {
            var change = false;
            var config = this.module.InviterConfig;
            var toolTips = Modules.Instance.Configuration.ShowToolTips;
            if (ImGui.Checkbox(this.Localizer.Localize("Enable"), ref config.Enable))
            {
                change = true;
            }

            if (toolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(this.Localizer.Localize("Automatically invite people to your party (doesn't work for CWLS)."));
            }

            ImGui.TextUnformatted(this.Localizer.Localize("Pattern:"));
            if (toolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(this.Localizer.Localize("Pattern of the chat message to trigger the invitation."));
            }

            if (ImGui.InputText("##textPattern", ref config.TextPattern, 256))
            {
                this.module.invitePattern = null;
                change = true;
            }

            ImGui.SameLine(ImGui.GetColumnWidth() - 120);
            ImGui.TextUnformatted(this.Localizer.Localize("Regex"));
            ImGui.AlignTextToFramePadding();
            ImGui.SameLine();
            if (ImGui.Checkbox("##regexMatch", ref config.RegexMatch))
            {
                change = true;
            }

            if (toolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(this.Localizer.Localize("Use regex to match the pattern to chat messages."));
            }

            ImGui.TextUnformatted(this.Localizer.Localize("Delay(ms):"));
            if (toolTips && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(this.Localizer.Localize("Delay the invitation after triggered."));
            }

            ImGui.SetNextItemWidth(150);
            if (ImGui.InputInt("##Delay", ref config.Delay, 10, 100))
            {
                change = true;
            }

            if (ImGui.Checkbox(this.Localizer.Localize("Print Debug Message"), ref config.PrintMessage))
            {
                change = true;
            }

            if (ImGui.Checkbox(this.Localizer.Localize("Print Error Message"), ref config.PrintError))
            {
                change = true;
            }

            if (change)
            {
                Configuration.Save();
            }
        }

        private void DrawFilters()
        {
            ImGui.Columns(4, "FiltersTable", true);
            var config = this.module.InviterConfig;
            foreach (XivChatType chatType in Enum.GetValues<XivChatType>())
            {
                if (config.HiddenChatType?.IndexOf(chatType) != -1)
                {
                    continue;
                }

                var chatTypeName = Enum.GetName(chatType);
                bool checkboxClicked = config.FilteredChannels?.IndexOf(chatType) == -1;
                if (ImGui.Checkbox(this.Localizer.Localize(chatTypeName) + "##filter", ref checkboxClicked))
                {
                    config.FilteredChannels = config.FilteredChannels?.Distinct().ToList() ?? new();
                    if (checkboxClicked)
                    {
                        if (config.FilteredChannels.IndexOf(chatType) != -1)
                        {
                            config.FilteredChannels.Remove(chatType);
                        }
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