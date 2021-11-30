// <copyright file="MacroModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro;

using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Logging;

/// <summary>
///     Implements the macro module.
/// </summary>
internal sealed class MacroModule : IModule
{
    /// <summary>
    ///     The name of the module.
    /// </summary>
    internal const string ModuleName = "Macro";

    private readonly MacroConfigTab configTab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="Manager.ModuleManager" /> added by DI.</param>
    /// <param name="configTab"><see cref="MacroConfigTab" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    public MacroModule(IModuleManager moduleManager, MacroConfigTab configTab, Configuration configuration)
    {
        this.configTab = configTab;
        this.Configuration = configuration.Macro!;

        moduleManager.Register(this, this.Configuration.Enabled);
        PluginLog.LogDebug($"Module 'Macro' init. {this.Configuration}");
    }

    public string Name => ModuleName;

    /// <summary>
    ///     Gets the configuration.
    /// </summary>
    internal MacroConfiguration Configuration { get; }

    /// <inheritdoc />
    public void Draw() => this.configTab.DrawTab();

    public void Enable(bool state = true) => this.Configuration.Enabled = state;
}