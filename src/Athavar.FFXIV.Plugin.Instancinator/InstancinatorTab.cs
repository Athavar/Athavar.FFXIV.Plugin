// <copyright file="InstancinatorTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Instancinator;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.UI;
using ImGuiNET;

internal class InstancinatorTab : Tab, IInstancinatorTab
{
    public InstancinatorTab(Configuration configuration) => this.Configuration = configuration.Instancinator!;

    public override string Name => InstancinatorModule.ModuleName;

    public override string Identifier => "instancinator";

    /// <summary>
    ///     Gets the configuration.
    /// </summary>
    private InstancinatorConfiguration Configuration { get; }

    /// <inheritdoc />
    public override void Draw()
    {
        ImGui.SetNextItemWidth(100f);
        if (ImGui.InputText("Interact keycode", ref this.Configuration.KeyCode, 64))
        {
            if (this.Configuration.KeyCode.Length == 0)
            {
                this.Configuration.KeyCode = Native.KeyCode.NumPad0.ToString();
            }

            this.Configuration.Save();
        }

        ImGui.SetNextItemWidth(100f);
        if (ImGui.DragInt("Extra delay, MS", ref this.Configuration.ExtraDelay, 1f, 0, 2000))
        {
            if (this.Configuration.ExtraDelay < 0)
            {
                this.Configuration.ExtraDelay = 0;
            }

            this.Configuration.Save();
        }
    }
}