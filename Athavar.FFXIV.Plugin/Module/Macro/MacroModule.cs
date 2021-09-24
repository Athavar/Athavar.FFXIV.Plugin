using Athavar.FFXIV.Plugin;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.IO;
using System.Reflection;

namespace SomethingNeedDoing
{
    internal sealed class MacroModule
    {
        public string Name => "Something Need Doing";
        public string Command => "/pcraft";

        internal MacroConfiguration Configuration;
        internal MacroAddressResolver Address;
        internal ChatManager ChatManager;
        internal MacroManager MacroManager;

        private readonly MacroUI PluginUi;

        public MacroModule(MacroConfiguration configuration)
        {
            Configuration = configuration;

            Address = new MacroAddressResolver();
            Address.Setup();

            ChatManager = new ChatManager(this);
            MacroManager = new MacroManager(this);
            PluginUi = new MacroUI(this);
        }

        public void Dispose()
        {
            ChatManager.Dispose();
            MacroManager.Dispose();
            PluginUi.Dispose();
        }

        internal void Draw() => PluginUi.UiBuilder_OnBuildUi();

        internal void SaveConfiguration() => DalamudBinding.PluginInterface.SavePluginConfig(Modules.Configuration);

        internal byte[] ReadResourceFile(params string[] filePathSegments)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new DllNotFoundException();
            var resourceFilePath = Path.Combine(assemblyFolder, Path.Combine(filePathSegments));
            return File.ReadAllBytes(resourceFilePath);
        }

        internal byte[] ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly() ?? throw new DllNotFoundException(); ;
            using Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException(resourceName);
            using BinaryReader reader = new(stream);
            return reader.ReadBytes((int)stream.Length);
        }
    }
}
