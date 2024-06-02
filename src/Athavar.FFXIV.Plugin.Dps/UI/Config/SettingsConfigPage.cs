// <copyright file="SettingsConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using ImGuiNET;

internal sealed class SettingsConfigPage : IConfigPage
{
    private static readonly string[] PartyTypeOptions = Enum.GetNames(typeof(PartyType));
    private readonly DpsConfiguration c;

    public SettingsConfigPage(DpsConfiguration dpsC) => this.c = dpsC;

    public string Name => "Settings";

    public IConfig GetDefault() => new DummyConfig();

    public IConfig GetConfig() => new DummyConfig();

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            ImGuiEx.Combo("Party Filter", this.c.PartyFilter, x => this.c.PartyFilter = x, PartyTypeOptions);

            ImGuiEx.DragInt("Text Refresh Interval", this.c.TextRefreshInterval, x => this.c.TextRefreshInterval = x, 100, 1, 5000);

            ImGui.EndChild();
        }
    }
}