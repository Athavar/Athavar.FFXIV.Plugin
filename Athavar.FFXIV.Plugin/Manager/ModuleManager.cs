// <copyright file="ModuleManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager;

using System.Collections.Generic;
using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;

/// <summary>
///     Manage instances of <see cref="IModule" />.
/// </summary>
internal class ModuleManager : IModuleManager
{
    private readonly Dictionary<string, bool> enabled = new();
    private readonly Dictionary<string, IModule> modules = new();

    private readonly Configuration configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModuleManager" /> class.
    /// </summary>
    public ModuleManager(Configuration configuration) => this.configuration = configuration;

    /// <inheritdoc />
    public IEnumerable<string> GetModuleNames() => this.modules.Where(m => !m.Value.Hidden).Select(m => m.Key);

    /// <inheritdoc />
    public bool IsEnables(string moduleName) => this.enabled.TryGetValue(moduleName, out var state) && state;

    /// <inheritdoc />
    public void Enable(string moduleName, bool state = true)
    {
        if (this.modules.TryGetValue(moduleName, out var mod))
        {
            this.enabled[moduleName] = state;
            mod.Enable(state);

            this.configuration.Save();
        }
    }

    /// <inheritdoc />
    public bool Register(IModule module, bool enableState)
    {
        this.enabled.TryAdd(module.Name, enableState);
        return this.modules.TryAdd(module.Name, module);
    }

    /// <summary>
    ///     Draw all module tabs.
    /// </summary>
    public void Draw()
    {
        foreach (var m in this.modules.Values.Where(m => this.enabled[m.Name]))
        {
            Plugin.CatchCrash(() => m.Draw());
        }
    }
}