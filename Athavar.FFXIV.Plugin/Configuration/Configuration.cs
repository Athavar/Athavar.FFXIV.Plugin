using Dalamud.Configuration;
using Inviter;
using Newtonsoft.Json;
using SomethingNeedDoing;
using System.IO;
using YesAlready;

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
            return config.Init();
        }

        public static void Save()
        {
            DalamudBinding.PluginInterface.SavePluginConfig(Modules.Configuration);
        }


        private Configuration()
        {
        }

        private Configuration Init()
        {
            this.Yes ??= new();
            this.Macro ??= new();
            (this.Inviter ??= new()).Init();
            return this;
        }

        public int Version { get; set; } = 1;

        public YesConfiguration? Yes { get; set; } = null!;

        public MacroConfiguration? Macro { get; set; } = null!;

        public InviterConfiguration? Inviter { get; set; } = null!;     

        public bool ShowToolTips { get; set; } = true;

        public Language Language { get; set; } = Language.EN;
    }
}
