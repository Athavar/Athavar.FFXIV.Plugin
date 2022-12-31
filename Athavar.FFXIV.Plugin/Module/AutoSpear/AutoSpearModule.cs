// <copyright file="AutoSpearModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Logging;

internal class AutoSpearModule : IModule
{
    private const string ModuleName = "AutoSpear";
    private readonly AutoSpear tab;

    public AutoSpearModule(IModuleManager moduleManager, Configuration configuration, AutoSpear windowHelper)
    {
        this.Configuration = configuration.AutoSpear!;

        moduleManager.Register(this, this.Configuration.Enabled);
        this.tab = windowHelper;
        PluginLog.LogDebug("Module 'AutoSpear' init");
    }

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public bool Hidden => false;

    /// <summary>
    ///     Gets the configuration.
    /// </summary>
    internal AutoSpearConfiguration Configuration { get; }

    public void Draw() => this.tab.DrawTab();

    public void Enable(bool state = true)
    {
        this.Configuration.Enabled = state;
        this.tab.IsOpen = state;
    }
}