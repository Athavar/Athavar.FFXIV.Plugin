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
        var keyCode = this.Configuration.KeyCode;
        if (ImGui.InputText("Interact keycode", ref keyCode, 64))
        {
            this.Configuration.KeyCode = keyCode;
            if (this.Configuration.KeyCode.Length == 0)
            {
                this.Configuration.KeyCode = Native.KeyCode.NumPad0.ToString();
            }

            this.Configuration.Save();
        }

        ImGui.SetNextItemWidth(100f);
        var delay = this.Configuration.ExtraDelay;
        if (ImGui.DragInt("Extra delay, MS", ref delay, 1f, 0, 2000))
        {
            this.Configuration.ExtraDelay = delay;
            if (this.Configuration.ExtraDelay < 0)
            {
                this.Configuration.ExtraDelay = 0;
            }

            this.Configuration.Save();
        }
    }
}