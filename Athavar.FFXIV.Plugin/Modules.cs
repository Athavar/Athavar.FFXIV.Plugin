using Dalamud.Logging;
using Inviter;
using SomethingNeedDoing;
using System;
using YesAlready;

namespace Athavar.FFXIV.Plugin
{
    internal class Modules : IDisposable
    {
        private IModule?[] modules;

        private static readonly Lazy<Modules> instance = new(() => new Modules(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

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

        public static Modules Instance => instance.Value;
        public static readonly Module[] ModuleValues = Enum.GetValues<Module>();

        private static Type GetModuleType(Module module) => module switch
        {
            Module.Yes => typeof(YesModule),
            Module.Macro => typeof(MacroModule),
            Module.Inviter => typeof(InviterModule),
            _ => throw new NotImplementedException(),
        };

        public Configuration Configuration { get; init; }
        public Localizer Localizer { get; init; }

        public void Dispose()
        {
            foreach (var mod in modules)
            {
                if (mod is not null)
                {
                    mod.Dispose();
                }
            }
        }

        public void Enable(Module module, bool enable)
        {
            PluginLog.Log("Change Module enable status. Module: {0} -> {1}", module, enable);
            var index = Array.IndexOf(ModuleValues, module);

            var mod = modules[index];

            if (enable)
            {
                this.Configuration.ModuleEnabled.Add(module);
                if (mod is null)
                {
                    var type = GetModuleType(module);
                    modules[index] = (IModule?)Activator.CreateInstance(type, this);
                }

            }
            else
            {
               var ff = this.Configuration.ModuleEnabled.Remove(module);
                PluginLog.Log(ff.ToString());

                if (mod is not null)
                {

                    mod.Dispose();
                    modules[index] = null;
                }
            }
        }

        public IModule? GetModule(Module module)
        {
            var index = Array.IndexOf(ModuleValues, module);

            return modules[index];
        }

        public void Draw()
        {

            foreach (var m in modules)
            {
                m?.Draw();
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
