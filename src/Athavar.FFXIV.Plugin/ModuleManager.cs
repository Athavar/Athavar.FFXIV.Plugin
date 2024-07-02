// <copyright file="ModuleManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System.Reflection;
using System.Runtime.Loader;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using Module = Athavar.FFXIV.Plugin.Models.Module;

/// <summary>
///     Manage instances of <see cref="Models.Module"/>.
/// </summary>
internal sealed class ModuleManager : IModuleManager, IDisposable
{
    private readonly Dictionary<string, ModuleDef> modules = new();

    private readonly IServiceProvider serviceProvider;
    private readonly IPluginLogger logger;

    private readonly string[] moduleAssemblies =
    {
        "Athavar.FFXIV.Plugin.AutoSpear",
        "Athavar.FFXIV.Plugin.Cheat",
        "Athavar.FFXIV.Plugin.CraftQueue",
        "Athavar.FFXIV.Plugin.Dps",
        "Athavar.FFXIV.Plugin.DutyHistory",
        "Athavar.FFXIV.Plugin.Importer",
        "Athavar.FFXIV.Plugin.Instancinator",
        "Athavar.FFXIV.Plugin.Macro",
        "Athavar.FFXIV.Plugin.OpcodeWizard",
        "Athavar.FFXIV.Plugin.SliceIsRight",
        "Athavar.FFXIV.Plugin.Yes",
    };

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModuleManager"/> class.
    /// </summary>
    /// <param name="serviceProvider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="logger"><see cref="IPluginLog"/> added by DI.</param>
    public ModuleManager(IServiceProvider serviceProvider, IPluginLogger logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public event IModuleManager.ModuleStateChange? StateChange;

    public void LoadModules()
    {
        var lockObject = new object();
        void LoadModule(Type moduleType)
        {
            var attribute = moduleType.GetCustomAttribute<ModuleAttribute>();
            if (attribute is null)
            {
                return;
            }

            if (attribute.Debug)
            {
#if !DEBUG
                return;
#endif
            }

            var definition = new ModuleDef(this, attribute.Name, attribute.HasTab, attribute.Hidden, moduleType)
            {
                Enabled = true,
            };

            if (attribute.ModuleConfigurationType is not null)
            {
                this.logger.Debug("Get configuration for module {0}", attribute.Name);
                var config = this.serviceProvider.GetRequiredService(attribute.ModuleConfigurationType);
                this.logger.Debug("Received configuration for module {0}", attribute.Name);
                if (config is BasicModuleConfig moduleConfig)
                {
                    definition.Enabled = moduleConfig.Enabled;
                    definition.Config = moduleConfig;
                }
            }

            bool result;
            lock (lockObject)
            {
                result = this.modules.TryAdd(attribute.Name, definition);
            }

            if (result)
            {
                if (definition.Enabled)
                {
                    object? m = null;
                    this.logger.Debug("Create Module Instance for {0}", attribute.Name);
                    try
                    {
                        m = this.CreateModule(moduleType);
                    }
                    catch (Exception ex)
                    {
                        definition.LoadingError = ex;
                        this.logger.Error(ex, "Error during load of {0}", attribute.Name);
                    }

                    if (m is Module module)
                    {
                        this.logger.Debug("Finish creating Module Instance for {0}", attribute.Name);
                        definition.Instance = module;
                        this.StateChange?.Invoke(module, definition.Data);
                    }
                }
            }
            else
            {
                this.logger.Verbose("{0} was already added", attribute.Name);
            }
        }

        var moduleType = typeof(Module);

        var context = AssemblyLoadContext.GetLoadContext(typeof(ModuleManager).Assembly);
        if (context is null)
        {
            return;
        }

        this.logger.Information("Load modules from AssemblyLoadContext: {0}", context.ToString());

        List<Type> moduleTypes = new();

        foreach (var assemblyName in this.moduleAssemblies)
        {
            try
            {
                var assembly = context.LoadFromAssemblyName(new AssemblyName(assemblyName));
                moduleTypes
                   .AddRange(
                        assembly
                           .GetTypes()
                           .Where(t => !t.IsAbstract && t.IsSubclassOf(moduleType)));
            }

            // ReSharper disable once RedundantCatchClause
            catch (Exception ex)
            {
                // ignored
#if DEBUG
                throw;
#endif
            }
        }

        this.logger.Information("Start loading modules");
        moduleTypes.AsParallel().WithDegreeOfParallelism(12).ForAll(LoadModule);
        this.logger.Information("Finish loading modules");
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetModuleNames() => this.modules.Where(m => !m.Value.Hidden).Select(m => m.Key);

    /// <inheritdoc/>
    public IEnumerable<IModuleManager.IModuleData> GetModuleData() => this.modules.Where(m => !m.Value.Hidden).Select(m => m.Value.Data).Order();

    /// <inheritdoc/>
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

    private object CreateModule(Type type) => ActivatorUtilities.CreateInstance(this.serviceProvider, type);

    private sealed class ModuleDef
    {
        public ModuleDef(ModuleManager manager, string name, bool hasTabs, bool hidden, Type type)
        {
            this.Manager = manager;
            this.Hidden = hidden;
            this.Type = type;
            this.Data = new ModuleData(this, name, hasTabs);
        }

        public bool Hidden { get; }

        internal Type Type { get; }

        internal ModuleManager Manager { get; }

        internal ModuleData Data { get; }

        internal bool Enabled { get; set; }

        internal Exception? LoadingError { get; set; }

        internal Module? Instance { get; set; }

        internal BasicModuleConfig? Config { get; set; }
    }

    private sealed record ModuleData(ModuleDef Def, string Name, bool HasTab) : IModuleManager.IModuleData, IComparable<ModuleData>, IComparable
    {
        public bool Enabled
        {
            get => this.Def.Enabled;
            set
            {
                if (this.Def.Instance is null)
                {
                    object? instance = null;
                    try
                    {
                        // Enable
                        instance = this.Def.Manager.CreateModule(this.Def.Type);
                    }
                    catch (Exception ex)
                    {
                        this.Def.LoadingError = ex;
                        this.Def.Manager.logger.Error(ex, "Error during load of {0}", this.Def.Data.Name);
                    }

                    if (instance is not Module module)
                    {
                        return;
                    }

                    this.Def.LoadingError = null;
                    this.Def.Instance = module;
                }

                this.Def.Enabled = value;
                this.Def.Instance.Enable(value);
                this.Def.Config?.Save();
                if (value)
                {
                    // Enable
                    this.Def.Instance.Init();
                }

                this.Def.Manager.StateChange?.Invoke(this.Def.Instance, this);
                if (!value)
                {
                    // Disable
                    if (this.Def.Instance is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    this.Def.Instance = null;
                }
            }
        }

        public bool TabEnabled
        {
            get => this.Def.Config?.TabEnabled ?? false;
            set
            {
                if (this.Def.Config is not null)
                {
                    this.Def.Config.TabEnabled = value;
                    this.Def.Config?.Save();

                    if (this.Def.Instance is not null)
                    {
                        this.Def.Manager.StateChange?.Invoke(this.Def.Instance, this);
                    }
                }
            }
        }

        public bool Loaded => this.Def.Instance is not null;

        public Exception? LoadingError => this.Def.LoadingError;

        public int CompareTo(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is ModuleData other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ModuleData)}");
        }

        public int CompareTo(ModuleData? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            var nameComparison = string.Compare(this.Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0)
            {
                return nameComparison;
            }

            return this.HasTab.CompareTo(other.HasTab);
        }
    }
}