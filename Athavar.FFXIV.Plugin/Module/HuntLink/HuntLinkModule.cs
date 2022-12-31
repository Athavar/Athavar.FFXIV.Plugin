// <copyright file="HuntLinkModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.HuntLink;

using Athavar.FFXIV.Plugin.Manager;
using Athavar.FFXIV.Plugin.Manager.Interface;

/// <summary>
///     Implements the hunt link module.
/// </summary>
internal class HuntLinkModule : IModule
{
    private const string ModuleName = "HuntLink";
    private readonly HuntLinkTab tab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HuntLinkModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="ModuleManager" /> added by DI.</param>
    /// <param name="huntLinkTab"><see cref="HuntLinkTab" /> added by DI.</param>
    public HuntLinkModule(IModuleManager moduleManager, HuntLinkTab huntLinkTab)
    {
        this.tab = huntLinkTab;
        moduleManager.Register(this, false);
    }

    public string Name => ModuleName;

    /// <inheritdoc />
    public bool Hidden => false;

    public void Dispose()
    {
    }

    public void Draw() => this.tab.DrawTab();

    public void Enable(bool state = true) => _ = state;
}