using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace Athavar.FFXIV.Plugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Athavar's Tools";

        private const string CommandName = "/ath";

        private CommandManager CommandManager { get; init; }
        private PluginUI PluginUi { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            DalamudBinding.Initialize(pluginInterface);
            _ = Modules.Instance;

            this.CommandManager = commandManager;

            this.PluginUi = new PluginUI(this);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open the Configuration of Athavar's Tools."
            });
        }

        public void Dispose()
        {
            Modules.Instance.Dispose();
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(CommandName);
        }

        internal static void SaveConfiguration()
        {
            Configuration.Save();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.OpenConfig();
        }
    }
}
