// <copyright file="CraftQueueModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Logging;

public class CraftQueueModule : IModule
{
    private const string ModuleName = "CraftQueue (Beta)";

    private readonly IDalamudServices dalamudServices;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftQueueModule" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="tab"><see cref="ICraftQueueTab" /> added by DI.</param>
    public CraftQueueModule(IDalamudServices dalamudServices, Configuration configuration, ICraftQueueTab tab)
    {
        this.dalamudServices = dalamudServices;
        this.Tab = tab;
        this.Configuration = configuration.CraftQueue!;

        PluginLog.LogDebug("Module 'CraftQueue' init");
    }

    /// <inheritdoc />
    public override bool Enabled => this.Configuration.Enabled;

    /// <inheritdoc />
    public override string Name => ModuleName;

    /// <inheritdoc />
    public override bool Hidden => false;

    /// <inheritdoc />
    public override ICraftQueueTab? Tab { get; }

    private CraftQueueConfiguration Configuration { get; }

    /// <inheritdoc />
    public override void Enable(bool state = true) => this.Configuration.Enabled = state;
}