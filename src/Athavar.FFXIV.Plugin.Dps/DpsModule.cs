// <copyright file="DpsModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Dps.UI;
using Dalamud.Interface;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

public class DpsModule : Module, IDisposable
{
    internal const string ModuleName = "DPS (Test)";

    private const ImGuiWindowFlags MainWindowFlags =
        ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.AlwaysAutoResize |
        ImGuiWindowFlags.NoBackground |
        ImGuiWindowFlags.NoInputs |
        ImGuiWindowFlags.NoBringToFrontOnFocus |
        ImGuiWindowFlags.NoSavedSettings;

    private readonly Vector2 origin = ImGui.GetMainViewport().Size / 2f;

    private readonly IServiceProvider provider;
    private readonly IDalamudServices services;
    private readonly NetworkHandler networkHandler;
    private readonly MeterManager meterManager;
    private IDpsTab? tab;

    public DpsModule(Configuration configuration, IServiceProvider provider, IDalamudServices services)
        : base(configuration)
    {
        this.services = services;
        this.provider = provider;

        this.networkHandler = provider.GetRequiredService<NetworkHandler>();
        this.networkHandler.Enable = this.Enabled;

        this.meterManager = provider.GetRequiredService<MeterManager>();
        this.meterManager.Setup(new Lazy<DpsTab>(() => this.Tab as DpsTab ?? throw new InvalidOperationException()));

        this.services.PluginInterface.UiBuilder.Draw += this.Draw;
    }

    public override IDpsTab Tab => this.tab ??= this.provider.GetRequiredService<IDpsTab>();

    public override string Name => ModuleName;

    /// <inheritdoc />
    public override (Func<Configuration, bool> Get, Action<bool, Configuration> Set) GetEnableStateAction()
    {
        bool Get(Configuration c) => this.Configuration.Dps!.Enabled;

        void Set(bool state, Configuration c)
        {
            this.Configuration.Dps!.Enabled = state;
            this.networkHandler.Enable = state;
        }

        return (Get, Set);
    }

    public void Dispose() => this.services.PluginInterface.UiBuilder.Draw -= this.Draw;

    private void Draw()
    {
        ImGuiHelpers.ForceNextWindowMainViewport();
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(ImGui.GetMainViewport().Size);
        if (ImGui.Begin("DpsModule_Root", MainWindowFlags))
        {
            foreach (var meter in this.meterManager.Meters)
            {
                meter.Draw(this.origin);
            }
        }

        ImGui.End();
    }
}