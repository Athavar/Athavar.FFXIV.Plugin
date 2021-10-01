using Athavar.FFXIV.Plugin;
using Dalamud.Logging;
using System;
using System.IO;
using System.Reflection;

namespace SomethingNeedDoing
{
    internal sealed class MacroModule : IModule
    {
        public string Name => "Something Need Doing";
        public string Command => "/pcraft";

        internal MacroConfiguration configuration;
        internal MacroAddressResolver Address;
        internal ChatManager ChatManager;
        internal MacroManager MacroManager;

        private readonly MacroUI PluginUi;

        public MacroModule(Modules modules)
        {
            this.configuration = modules.Configuration.Macro ??= new();
            PluginLog.LogDebug($"Module 'Macro' init. {configuration}");

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

        public void Draw() => PluginUi.UiBuilder_OnBuildUi();

        internal void SaveConfiguration() => Configuration.Save();

        internal byte[] ReadResourceFile(params string[] filePathSegments)
        {
            var assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
