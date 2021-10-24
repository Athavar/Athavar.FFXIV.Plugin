// <copyright file="Modules.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System;
    using Athavar.FFXIV.Plugin.Module.HuntLink;
    using Athavar.FFXIV.Plugin.Module.Macro;
    using Athavar.FFXIV.Plugin.Module.Yes;
    using Athavar.FFXIV.Plugin.Utils;
    using Dalamud.Logging;
    using Inviter;

    /// <summary>
    /// Manage instances of <see cref="IModule"/>.
    /// </summary>
    internal class Modules : IDisposable
    {
        public static readonly Module[] ModuleValues = Enum.GetValues<Module>();

        private static readonly Lazy<Modules> instance = new(() => new Modules(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private object lockobject = new();

        private IModule?[] modules;

        /// <summary>
        /// Initializes a new instance of the <see cref="Modules"/> class.
        /// </summary>
        public Modules()
        {
            this.Configuration = Configuration.Load();
            this.Localizer = new();
            this.EquipmentScanner = new();

            // init module array
            this.modules = new IModule?[ModuleValues.Length];
            for (int i = 0; i < ModuleValues.Length; i++)
            {
                this.modules[i] = null;
                var mod = ModuleValues[i];

                if (this.Configuration.ModuleEnabled.Contains(mod))
                {
                    this.Enable(mod, true);
                }
            }
        }

        /// <summary>
        /// Gets the Base <see cref="Plugin"/>.
        /// </summary>
        public static Plugin Base { get; private set; } = null!;

        /// <summary>
        /// Gets the object instance of this class.
        /// </summary>
        public static Modules Instance => instance.Value;

        /// <summary>
        /// Gets the <see cref="FFXIV.Plugin.Configuration"/>.
        /// </summary>
        public Configuration Configuration { get; init; }

        /// <summary>
        /// Gets the <see cref="FFXIV.Plugin.Localizer"/>.
        /// </summary>
        public Localizer Localizer { get; init; }

        /// <summary>
        /// Gets the <see cref="FFXIV.Plugin.Localizer"/>.
        /// </summary>
        public EquipmentScanner EquipmentScanner { get; init; }

        /// <summary>
        /// Init this object.
        /// </summary>
        /// <param name="plugin">The base <see cref="Plugin"/>.</param>
        public static void Init(Plugin plugin)
        {
            Base = plugin;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var mod in this.modules)
            {
                if (mod is not null)
                {
                    mod.Dispose();
                }
            }

            this.EquipmentScanner.Dispose();
        }

        /// <summary>
        /// Enables/Disable a module.
        /// </summary>
        /// <param name="module">The module.</param>
        /// <param name="enable">Indicates if thee module should be enabled or disabled.</param>
        public void Enable(Module module, bool enable)
        {
            lock (this.lockobject)
            {
                PluginLog.Log("Change Module enable status. Module: {0} -> {1}", module, enable);
                var index = Array.IndexOf(ModuleValues, module);

                var mod = this.modules[index];

                if (enable)
                {
                    if (!this.Configuration.ModuleEnabled.Contains(module))
                    {
                        this.Configuration.ModuleEnabled.Add(module);
                    }

                    if (mod is null)
                    {
                        var type = GetModuleType(module);
                        this.modules[index] = (IModule?)Activator.CreateInstance(type, this);
                    }

                }
                else
                {
                    var ff = this.Configuration.ModuleEnabled.Remove(module);
                    PluginLog.Log(ff.ToString());

                    if (mod is not null)
                    {

                        mod.Dispose();
                        this.modules[index] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Get the specific module.
        /// </summary>
        /// <param name="module">The <see cref="Module"/> assioated to the module.</param>
        /// <returns>returns the module.</returns>
        public IModule? GetModule(Module module)
        {
            lock (this.lockobject)
            {
                var index = Array.IndexOf(ModuleValues, module);

                return this.modules[index];
            }
        }

        /// <summary>
        /// Draw all module tabs.
        /// </summary>
        public void Draw()
        {
            lock (this.lockobject)
            {
                foreach (var m in this.modules)
                {
                    m?.Draw();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="FFXIV.Plugin.Configuration"/>.
        /// </summary>
        private static Type GetModuleType(Module module) => module switch
        {
            Module.Yes => typeof(YesModule),
            Module.Macro => typeof(MacroModule),
            Module.Inviter => typeof(InviterModule),
#if DEBUG
            Module.HuntLink => typeof(HuntLinkModule),
#endif
            _ => throw new NotImplementedException(),
        };

        public enum Module
        {
            Macro = 1,
            Yes = 2,
            Inviter = 3,
#if DEBUG
            HuntLink = 4,
#endif
        }
    }
}
