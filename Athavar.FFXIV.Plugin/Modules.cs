namespace Athavar.FFXIV.Plugin
{
    using System;
    using Athavar.FFXIV.Plugin.Module.Yes;
    using Dalamud.Logging;
    using Inviter;
    using SomethingNeedDoing;

    internal class Modules : IDisposable
    {
        private IModule?[] modules;

        private static readonly Lazy<Modules> instance = new(() => new Modules(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        private object lockobject = new();

        public Modules()
        {
            this.Configuration = Configuration.Load();
            this.Localizer = new Localizer();

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

        public static Plugin Base { get; private set; }

        public static Modules Instance => instance.Value;

        public static readonly Module[] ModuleValues = Enum.GetValues<Module>();

        public Configuration Configuration { get; init; }

        public Localizer Localizer { get; init; }

        public static void Init(Plugin plugin)
        {
            Base = plugin;
        }

        private static Type GetModuleType(Module module) => module switch
        {
            Module.Yes => typeof(YesModule),
            Module.Macro => typeof(MacroModule),
            Module.Inviter => typeof(InviterModule),
            _ => throw new NotImplementedException(),
        };

        public void Dispose()
        {
            foreach (var mod in this.modules)
            {
                if (mod is not null)
                {
                    mod.Dispose();
                }
            }
        }

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

        public IModule? GetModule(Module module)
        {
            lock (this.lockobject)
            {
                var index = Array.IndexOf(ModuleValues, module);

                return this.modules[index];
            }
        }

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

        public enum Module
        {
            Macro = 1,
            Yes = 2,
            Inviter = 3,
        }
    }
}
