// <copyright file="ModuleManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;
using Module = Athavar.FFXIV.Plugin.Common.Module;

/// <summary>
///     Manage instances of <see cref="Common.Module" />.
/// </summary>
internal class ModuleManager : IModuleManager, IDisposable
{
    private readonly Dictionary<string, ModuleDef> modules = new();

    private readonly Configuration configuration;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModuleManager" /> class.
    /// </summary>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider" /> added by DI.</param>
    public ModuleManager(Configuration configuration, IServiceProvider serviceProvider)
    {
        this.configuration = configuration;
        this.serviceProvider = serviceProvider;
    }

    public event IModuleManager.ModuleStateChange? StateChange;

    public void LoadModules()
    {
        var moduleType = typeof(Module);

        var context = AssemblyLoadContext.GetLoadContext(typeof(ModuleManager).Assembly);
        if (context is null)
        {
            return;
        }

        PluginLog.Information("Load modules from AssemblyLoadContext: {0}", context.ToString());

        foreach (var mType in context.Assemblies
           .Where(a => !a.IsDynamic)
           .Where(a => a.GetName().Name?.StartsWith("Athavar.FFXIV.Plugin") ?? false)
           .SelectMany(a => a.GetTypes())
           .Where(t => !t.IsAbstract && t.IsSubclassOf(moduleType)))
        {
            var attribute = mType.GetCustomAttribute<ModuleAttribute>();
            if (attribute is null)
            {
                continue;
            }

            if (attribute.Debug)
            {
#if !DEBUG
                continue;
#endif
            }

            var enabled = true;
            if (attribute.ModuleConfigurationType is not null)
            {
                var config = this.configuration.GetBasicModuleConfig(attribute.ModuleConfigurationType);
                enabled = config?.Enabled ?? enabled;
            }

            var definition = new ModuleDef(attribute.Hidden, mType)
            {
                Enabled = enabled,
            };

            if (this.modules.TryAdd(attribute.Name, definition))
            {
                if (enabled)
                {
                    var m = ActivatorUtilities.CreateInstance(this.serviceProvider, mType);
                    if (m is Module module)
                    {
                        definition.Instance = module;
                        this.StateChange?.Invoke(module);
                    }
                }
            }
            else
            {
                PluginLog.LogVerbose("{0} was already added", attribute.Name);
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> GetModuleNames() => this.modules.Where(m => !m.Value.Hidden).Select(m => m.Key);

    /// <inheritdoc />
    public bool IsEnables(string moduleName) => this.modules.TryGetValue(moduleName, out var mod) && mod.Enabled;

    /// <inheritdoc />
    public void Enable(string moduleName, bool state = true)
    {
        if (this.modules.TryGetValue(moduleName, out var mod))
        {
            if (mod.Instance is null)
            {
                var instance = ActivatorUtilities.CreateInstance(this.serviceProvider, mod.Type);
                if (instance is not Module module)
                {
                    return;
                }

                mod.Instance = module;
            }

            mod.Enabled = state;
            mod.Instance.Enable(state);
            this.configuration.Save();
            if (state)
            {
                mod.Instance.Init();
            }

            this.StateChange?.Invoke(mod.Instance);
            if (!state)
            {
                if (mod.Instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                mod.Instance = null;
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var moduleDef in this.modules)
        {
            if (moduleDef.Value.Instance is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private record ModuleDef(bool Hidden, Type Type)
    {
        public bool Enabled { get; set; }

        public Module? Instance { get; set; }
    }
}