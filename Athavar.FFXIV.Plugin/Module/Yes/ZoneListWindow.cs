namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System.Linq;
    using System.Numerics;
    using Dalamud.Interface.Windowing;
    using ImGuiNET;

    /// <summary>
    /// Zone list window.
    /// </summary>
    internal class ZoneListWindow : Window
    {
        private readonly YesModule module;
        private bool sortZoneByName = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneListWindow"/> class.
        /// </summary>
        /// <param name="module">The <see cref="YesModule"/>.</param>
        public ZoneListWindow(YesModule module)
            : base("Yes Module Zone List")
        {
            this.module = module;
            this.Size = new Vector2(525, 600);
            this.SizeCondition = ImGuiCond.FirstUseEver;
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
            ImGui.Text($"Current ID: {DalamudBinding.ClientState.TerritoryType}");

            ImGui.Checkbox("Sort by Name", ref this.sortZoneByName);

            ImGui.Columns(2);

            ImGui.Text("ID");
            ImGui.NextColumn();

            ImGui.Text("Name");
            ImGui.NextColumn();

            ImGui.Separator();

            var names = this.module.TerritoryNames.AsEnumerable();

            if (this.sortZoneByName)
            {
                names = names.ToList().OrderBy(kvp => kvp.Value);
            }

            foreach (var kvp in names)
            {
                ImGui.Text($"{kvp.Key}");
                ImGui.NextColumn();

                ImGui.Text($"{kvp.Value}");
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }
    }
}
