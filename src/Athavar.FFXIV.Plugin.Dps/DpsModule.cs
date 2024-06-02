// <copyright file="DpsModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;

[Module(ModuleName, ModuleConfigurationType = typeof(DpsConfiguration), HasTab = true)]
internal sealed class DpsModule : Module<DpsTab, DpsConfiguration>
{
    internal const string ModuleName = "DPS";

    private readonly IServiceProvider provider;
    private readonly IDalamudServices services;
    private readonly NetworkHandler networkHandler;
    private MeterManager? meterManager;

    public DpsModule(DpsConfiguration configuration, IServiceProvider provider, IDalamudServices services)
        : base(configuration)
    {
        this.services = services;
        this.provider = provider;

        this.networkHandler = provider.GetRequiredService<NetworkHandler>();
        this.networkHandler.Enable = this.Enabled;
    }

    public override string Name => ModuleName;

    public override void Dispose()
    {
        base.Dispose();
        this.meterManager?.Dispose();
        this.services.PluginLogger.Verbose("Dispose Dps");
    }

    /// <inheritdoc/>
    protected override void OnEnabled() => this.networkHandler.Enable = true;

    /// <inheritdoc/>
    protected override void OnDisabled() => this.networkHandler.Enable = false;

    protected override DpsTab InitTab()
    {
        this.meterManager = ActivatorUtilities.CreateInstance<MeterManager>(this.provider);
        this.meterManager.Setup(new Lazy<DpsTab>(() => this.Tab ?? throw new InvalidOperationException()));
        return new DpsTab(this.provider, this.services, this.meterManager);
    }
}