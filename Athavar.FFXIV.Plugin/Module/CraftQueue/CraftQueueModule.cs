// <copyright file="CraftQueueModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Logging;

internal class CraftQueueModule : IModule
{
    private const string ModuleName = "CraftQueue (Beta)";

    private readonly IDalamudServices dalamudServices;
    private readonly CraftQueueTab tab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftQueueModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="IModuleManager" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="tab"><see cref="CraftQueueTab" /> added by DI.</param>
    public CraftQueueModule(IModuleManager moduleManager, IDalamudServices dalamudServices, Configuration configuration, CraftQueueTab tab)
    {
        this.dalamudServices = dalamudServices;
        this.tab = tab;
        this.Configuration = configuration.CraftQueue!;
        moduleManager.Register(this, this.Configuration.Enabled);

        PluginLog.LogDebug("Module 'CraftQueue' init");
    }

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public bool Hidden => false;

    private CraftQueueConfiguration Configuration { get; }

    /// <inheritdoc />
    public void Draw() => this.tab.DrawTab();

    /// <inheritdoc />
    public void Enable(bool state = true) => this.Configuration.Enabled = state;
}