// <copyright file="Plugin.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System;
    using System.Threading.Tasks;

    using ClickLib;
    using Dalamud.Game.Command;
    using Dalamud.Interface.Windowing;
    using Dalamud.IoC;
    using Dalamud.Logging;
    using Dalamud.Plugin;

    /// <summary>
    /// Main plugin implementation.
    /// </summary>
    public sealed class Plugin : IDalamudPlugin
    {
        private const string CommandName = "/ath";

        private readonly WindowSystem windowSystem;
        private readonly PluginWindow pluginWindow;

        /// <summary>
        /// Lock object for <see cref="WindowSystem"/>.
        /// </summary>
        private readonly object wSLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="pluginInterface">Dalamud plugin interface.</param>
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            DalamudBinding.Initialize(pluginInterface);
            Click.Initialize();
            Modules.Init(this);

            this.windowSystem = new("Athavar.FFXIV.Plugin");

            this.pluginWindow = new PluginWindow(this);
            this.windowSystem.AddWindow(this.pluginWindow);

            DalamudBinding.PluginInterface.UiBuilder.Draw += this.DrawWindow;
            DalamudBinding.PluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;

            DalamudBinding.CommandManager.AddHandler(CommandName, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Open the Configuration of Athavar's Tools.",
            });
        }

        /// <inheritdoc/>
        public string Name => "Athavar's Tools";

        /// <summary>
        /// Gets the dalamud windows system.
        /// </summary>
        internal PluginWindow PluginWindow => this.pluginWindow;

        /// <inheritdoc/>
        public void Dispose()
        {
            SaveConfiguration();

            DalamudBinding.CommandManager.RemoveHandler(CommandName);
            DalamudBinding.PluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
            DalamudBinding.PluginInterface.UiBuilder.Draw -= this.DrawWindow;

            Configuration.Dispose();
            Modules.Instance.Dispose();

            // remove all remaining windows.
            this.windowSystem.RemoveAllWindows();
        }

        /// <summary>
        /// Change something in the <see cref="WindowSystem"/>.
        /// </summary>
        /// <param name="action">The action that do the change.</param>
        internal void ChangeWindowSystem(Action<WindowSystem> action)
        {
            Task.Run(() =>
            {
                lock (this.wSLock)
                {
                    try
                    {
                        action.Invoke(this.windowSystem);
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            });
        }

        /// <summary>
        /// Save the plugin configuration.
        /// </summary>
        internal static void SaveConfiguration()
        {
            Configuration.Save();
        }

        /// <summary>
        /// Try to catch all exception.
        /// </summary>
        /// <param name="action">Action that can throw exception.</param>
        internal static void CatchCrash(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }
        }

        private void DrawWindow()
        {
            lock (this.wSLock)
            {
                this.windowSystem.Draw();
            }
        }

        private void OnOpenConfigUi()
        {
            this.pluginWindow.Toggle();
        }

        private void OnCommand(string command, string args)
        {
            this.pluginWindow.Toggle();
        }
    }
}
