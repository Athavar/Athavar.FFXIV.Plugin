// <copyright file="Configuration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System.Collections.Generic;
    using System.IO;
    using System.Timers;

    using Dalamud.Configuration;
    using Dalamud.Logging;
    using Newtonsoft.Json;

    internal class Configuration : IPluginConfiguration
    {
        private static Timer? saveTimer;

        public static Configuration Load()
        {
            var pluginConfigPath = DalamudBinding.PluginInterface.ConfigFile;

            Configuration config;
            if (!pluginConfigPath.Exists)
            {
                config = new Configuration();
            }
            else
            {
                pluginConfigPath.CopyTo($"{pluginConfigPath.FullName}.bak", true);
                config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(pluginConfigPath.FullName)) ?? new Configuration();
            }

            PluginLog.Debug("Load Configuration.");
            return config.Init();
        }

        /// <summary>
        /// Save the configuration.
        /// </summary>
        /// <param name="instant">Indicate if the configuration should be saved instant.</param>
        public static void Save(bool instant = false)
        {
            if (saveTimer is null)
            {
                saveTimer = new();
                saveTimer.Interval = 2500;
                saveTimer.AutoReset = false;
                saveTimer.Elapsed += (_, _) => Save(true);
            }

            if (instant)
            {
                saveTimer.Stop();
                PluginLog.LogDebug("Save Configuration");
                DalamudBinding.PluginInterface.SavePluginConfig(Modules.Instance.Configuration);
            }
            else if (!saveTimer.Enabled)
            {
                PluginLog.LogDebug("Start delayed configuration save.");
                saveTimer.Start();
            }
        }

        public static void Dispose()
        {
            saveTimer?.Dispose();
            saveTimer = null;
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

        /// <inheritdoc/>
        public int Version { get; set; } = 1;

        public YesConfiguration? Yes { get; set; } = null!;

        public MacroConfiguration? Macro { get; set; } = null!;

        public InviterConfiguration? Inviter { get; set; } = null!;

        public bool ShowToolTips { get; set; } = true;

        public Language Language { get; set; } = Language.EN;

        public List<Modules.Module> ModuleEnabled { get; set; } = null!;
    }
}
