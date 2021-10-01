using Dalamud.Configuration;
using Dalamud.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using static Athavar.FFXIV.Plugin.Modules;

namespace Athavar.FFXIV.Plugin
{
    internal class Configuration : IPluginConfiguration
    {
        public static Configuration Load()
        {
            var pluginConfigPath = DalamudBinding.PluginInterface.ConfigFile;

            Configuration config;
            if (!pluginConfigPath.Exists)
                config = new Configuration();
            else
                config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(pluginConfigPath.FullName)) ?? new Configuration();

            PluginLog.Debug("Load Configuration.");
            return config.Init();
        }

        public static void Save()
        {
            DalamudBinding.PluginInterface.SavePluginConfig(Modules.Instance.Configuration);
        }


        private Configuration()
        {
        }

        private Configuration Init()
        {
            this.Yes ??= new();
            this.Macro ??= new();
            (this.Inviter ??= new()).Init();
            this.ModuleEnabled ??= new();

            return this;
        }

        public int Version { get; set; } = 1;

        public YesConfiguration? Yes { get; set; } = null!;

        public MacroConfiguration? Macro { get; set; } = null!;

        public InviterConfiguration? Inviter { get; set; } = null!;     

        public bool ShowToolTips { get; set; } = true;

        public Language Language { get; set; } = Language.EN;

        public List<Module> ModuleEnabled { get; set; } = null!;
    }
}
