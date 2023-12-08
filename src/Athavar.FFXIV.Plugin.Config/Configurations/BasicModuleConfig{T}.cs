// <copyright file="BasicModuleConfig{T}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;

public class BasicModuleConfig<T> : BasicModuleConfig
    where T : BasicModuleConfig<T>, new()
{
    [JsonIgnore]
    private DirectoryInfo? configDirectory;

    [JsonIgnore]
    private IPluginLogger? logger;

    [JsonIgnore]
    private Timer? saveTimer;

    [JsonIgnore]
    private bool ConfigError { get; set; }

    public static IServiceCollection AddToDependencyInjection(IServiceCollection provider)
    {
        provider.AddSingleton<T>(o =>
        {
            var pi = o.GetRequiredService<DalamudPluginInterface>();
            var log = o.GetRequiredService<IPluginLogger>();

            return Load(pi.ConfigDirectory, log);
        });
        return provider;
    }

    public static T Load(DirectoryInfo configDirectory, IPluginLogger log)
    {
        bool TryLoad(string f, [NotNullWhen(true)] ref T? c)
        {
            if (File.Exists(f))
            {
#if DEBUG
                log.Information("Load file {0} for configuration {1}", f, GetConfigName());
#endif
                var data = File.ReadAllText(f);
                try
                {
                    c = (T?)JsonSerializer.Deserialize(data, typeof(T), ConfigurationJsonSerializerContext.Default);
                    if (c is not null)
                    {
                        return true;
                    }
                }
                catch (Exception e)
                {
                    log.Error(e, "Error during loading file {0} for configuration {1}", f, GetConfigName());
                    c = new T
                    {
                        ConfigError = true,
                    };
                }
            }

            return false;
        }

        var file = GetConfigFilePaths(configDirectory.FullName);

        T? config = null;
        if (!TryLoad(file.File, ref config) && !TryLoad(file.NewFile, ref config) && !TryLoad(file.BackupFile, ref config) && config is null)
        {
            // files don't exists. Create new configuration.
            config = new T();
        }

        if (config.ConfigError)
        {
            log.Warning("Fail to load any configuration for {0} but files exists. Changes will not be saved. Please correct or delete existing files.", GetConfigName());
        }

        config.configDirectory = configDirectory;
        config.logger = log;
        return config;
    }

    public override void Save(bool instant = false)
    {
        if (this.configDirectory is null || this.ConfigError)
        {
            return;
        }

        if (this.saveTimer is null)
        {
            this.saveTimer = new Timer();
            this.saveTimer.Interval = 10000;
#if DEBUG
            this.saveTimer.Interval = 2500;
#endif
            this.saveTimer.AutoReset = false;
            this.saveTimer.Elapsed += (_, _) => this.Save(true);
        }

        if (instant)
        {
            this.saveTimer.Stop();
            try
            {
                var data = JsonSerializer.Serialize(this, typeof(T), ConfigurationJsonSerializerContext.Default);

                var directory = this.configDirectory.FullName;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var paths = GetConfigFilePaths(this.configDirectory.FullName);

                // create new File
                File.WriteAllText(paths.NewFile, data);

                if (File.Exists(paths.File))
                {
                    // move current to backup
                    File.Move(paths.File, paths.BackupFile, true);
                }

                // move new to current
                File.Move(paths.NewFile, paths.File, true);
            }
            catch (Exception e)
            {
                this.logger?.Error(e, "Error during saving of configuration");
            }

#if DEBUG
            this.logger?.Information("Save {0} Successful", typeof(T).Name);
#endif
        }
        else if (!this.saveTimer.Enabled)
        {
            this.saveTimer.Start();
#if DEBUG
            this.logger?.Information("Save {0} Triggered", typeof(T).Name);
#endif
        }
    }

    /// <summary>
    ///     Setup <see cref="InstancinatorConfiguration"/>. Only used for migration.
    /// </summary>
    /// <param name="directoryInfo">The configuration directory.</param>
    internal void Setup(DirectoryInfo directoryInfo)
    {
        // migration only
        this.configDirectory = directoryInfo;
        this.Save(true);
    }

    private static (string File, string NewFile, string BackupFile) GetConfigFilePaths(string directory) => (Path.Combine(directory, GetConfigFileName()), Path.Combine(directory, GetConfigFileNewName()), Path.Combine(directory, GetConfigFileBackupName()));

    private static string GetConfigFileName() => $"{GetConfigName()}.json";

    private static string GetConfigFileBackupName() => $"{GetConfigName()}.bak.json";

    private static string GetConfigFileNewName() => $"{GetConfigName()}.new.json";

    private static string GetConfigName() => typeof(T).Name;
}