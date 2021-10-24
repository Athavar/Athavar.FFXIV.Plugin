// <copyright file="MacroModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro
{
    using Athavar.FFXIV.Plugin;
    using Dalamud.Logging;

    /// <summary>
    /// Implements the macro mmodule.
    /// </summary>
    internal sealed class MacroModule : IModule
    {
        private readonly MacroConfiguration configuration;
        private readonly MacroAddressResolver address;
        private readonly ChatManager chatManager;
        private readonly MacroManager macroManager;

        private readonly MacroConfigTab pluginUi;

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroModule"/> class.
        /// </summary>
        /// <param name="modules">The other <see cref="Modules"/>.</param>
        public MacroModule(Modules modules)
        {
            this.configuration = modules.Configuration.Macro ??= new();
            PluginLog.LogDebug($"Module 'Macro' init. {this.configuration}");

            this.address = new MacroAddressResolver();
            this.address.Setup();

            this.chatManager = new ChatManager(this);
            this.macroManager = new MacroManager(this);
            this.pluginUi = new MacroConfigTab(this);
        }

        /// <summary>
        /// Gets the Macro Address Resolver.
        /// </summary>
        internal MacroAddressResolver Address => this.address;

        /// <summary>
        /// Gets the Chat manager.
        /// </summary>
        internal ChatManager ChatManager => this.chatManager;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        internal MacroConfiguration Configuration => this.configuration;

        /// <summary>
        /// Gets the Macro manager.
        /// </summary>
        internal MacroManager MacroManager => this.macroManager;

        /// <inheritdoc/>
        public void Dispose()
        {
            this.chatManager.Dispose();
            this.macroManager.Dispose();
        }

        /// <inheritdoc/>
        public void Draw() => this.pluginUi.DrawTab();

        /// <summary>
        /// Save the configuration.
        /// </summary>
        internal void SaveConfiguration() => Athavar.FFXIV.Plugin.Configuration.Save();

    }
}
