// <copyright file="MeterManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI;
using Dalamud.Interface;
using ImGuiNET;

internal class MeterManager : IDisposable
{
    private const ImGuiWindowFlags MainWindowFlags =
        ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.AlwaysAutoResize |
        ImGuiWindowFlags.NoBackground |
        ImGuiWindowFlags.NoInputs |
        ImGuiWindowFlags.NoBringToFrontOnFocus |
        ImGuiWindowFlags.NoSavedSettings;

    private readonly IServiceProvider provider;
    private readonly DpsConfiguration configuration;
    private readonly IDalamudServices services;
    private readonly IPluginWindow pluginWindow;

    private readonly Vector2 origin = ImGui.GetMainViewport().Size / 2f;
    private Lazy<DpsTab>? tab;

    public MeterManager(Configuration configuration, IServiceProvider provider, IDalamudServices services, IPluginWindow pluginWindow)
    {
        this.configuration = configuration.Dps!;
        this.provider = provider;
        this.services = services;
        this.pluginWindow = pluginWindow;
        this.Load();

        this.services.PluginInterface.UiBuilder.Draw += this.Draw;
    }

    public List<MeterWindow> Meters { get; set; } = new();

    public void DeleteMeter(MeterWindow meter)
    {
        this.Meters.Remove(meter);
        this.configuration.Meters.Remove(meter.Config);
        this.Save();
    }

    public void AddMeter(MeterWindow meter)
    {
        this.Meters.Add(meter);
        this.configuration.Meters.Add(meter.Config);
        this.Save();
    }

    public void ConfigureMeter(MeterWindow meterWindow)
    {
        if (!this.pluginWindow.IsOpen)
        {
            this.pluginWindow.Toggle();
        }

        this.tab?.Value.PushConfig(meterWindow);
    }

    public void Save() => this.configuration.Save();

    public void Dispose() => this.services.PluginInterface.UiBuilder.Draw -= this.Draw;

    internal void Setup(Lazy<DpsTab> t) => this.tab = t;

    private void Load()
    {
        var list = new List<MeterWindow>();
        foreach (var meter in this.configuration.Meters)
        {
            list.Add(new MeterWindow(meter, this.provider, this));
        }

        this.Meters = list;
    }

    private void Draw()
    {
        if (!this.services.ClientState.IsLoggedIn)
        {
            return;
        }

        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
        if (ImGui.Begin("DpsModule_Root", MainWindowFlags))
        {
            foreach (var meter in this.Meters)
            {
                meter.Draw(this.origin);
            }
        }

        ImGui.End();
    }
}