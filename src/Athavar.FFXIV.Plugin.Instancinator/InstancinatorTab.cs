// <copyright file="InstancinatorTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Instancinator;

using Athavar.FFXIV.Plugin.Common.UI;
using ImGuiNET;

internal sealed class InstancinatorTab : Tab
{
    public InstancinatorTab(InstancinatorConfiguration configuration) => this.Configuration = configuration;

    public override string Name => InstancinatorModule.ModuleName;

    public override string Identifier => "instancinator";

    /// <summary>
    ///     Gets the configuration.
    /// </summary>
    private InstancinatorConfiguration Configuration { get; }

    /// <inheritdoc />
    public override void Draw()
    {
        ImGui.TextUnformatted("Require's the Yes Module enabled");
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