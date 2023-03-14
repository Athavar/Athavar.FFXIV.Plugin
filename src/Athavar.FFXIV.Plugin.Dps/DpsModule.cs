// <copyright file="DpsModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(DpsConfiguration))]
internal sealed class DpsModule : Module<DpsTab, DpsConfiguration>
{
    internal const string ModuleName = "DPS (Test)";

    private readonly IServiceProvider provider;
    private readonly IDalamudServices services;
    private readonly NetworkHandler networkHandler;
    private MeterManager? meterManager;

    public DpsModule(Configuration configuration, IServiceProvider provider, IDalamudServices services)
        : base(configuration, configuration.Dps!)
    {
        this.services = services;
        this.provider = provider;

        this.networkHandler = provider.GetRequiredService<NetworkHandler>();
        this.networkHandler.Enable = this.Enabled;
    }

    public override string Name => ModuleName;

    /// <inheritdoc />
    public override (Func<bool> Get, Action<bool> Set) GetEnableStateAction()
    {
        bool Get() => this.Configuration.Dps!.Enabled;

        void Set(bool state)
        {
            this.Configuration.Dps!.Enabled = state;
            this.networkHandler.Enable = state;
        }

        return (Get, Set);
    }

    public override void Dispose()
    {
        base.Dispose();
        this.meterManager?.Dispose();
        PluginLog.LogVerbose("Dispose Dps");
    }

    protected override DpsTab InitTab()
    {
        this.meterManager = ActivatorUtilities.CreateInstance<MeterManager>(this.provider);
        this.meterManager.Setup(new Lazy<DpsTab>(() => this.Tab ?? throw new InvalidOperationException()));
        return new DpsTab(this.provider, this.services, this.meterManager);
    }
}