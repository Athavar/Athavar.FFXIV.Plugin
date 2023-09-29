// <copyright file="BasicModuleConfig{T}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using Dalamud.Logging;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;

public class BasicModuleConfig<T> : BasicModuleConfig
    where T : BasicModuleConfig<T>, new()
{
    // ReSharper disable once StaticMemberInGenericType
    [JsonIgnore]
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        TypeInfoResolver = new PrivateConstructorContractResolver(),
        IncludeFields = true,
    };

    [JsonIgnore]
    private DirectoryInfo? configDirectory;

    [JsonIgnore]
    private Timer? saveTimer;

    [JsonIgnore]
    private bool ConfigError { get; set; }

    public static IServiceCollection AddToDependencyInjection(IServiceCollection provider)
    {
        provider.AddSingleton<T>(o =>
        {
            var pi = o.GetRequiredService<DalamudPluginInterface>();

            return Load(pi);
        });
        return provider;
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
                var data = JsonSerializer.Serialize(this, typeof(T), SerializerOptions);

                var directory = this.configDirectory.FullName;
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var file = Path.Combine(this.configDirectory.FullName, GetConfigFileName());
                File.WriteAllText(file, data);
            }
            catch (Exception e)
            {
                PluginLog.LogError(e, "Error during saving of configuration");
            }

#if DEBUG
            PluginLog.Information("Save {0} Successful", typeof(T).Name);
#endif
        }
        else if (!this.saveTimer.Enabled)
        {
            this.saveTimer.Start();
#if DEBUG
            PluginLog.Information("Save {0} Triggered", typeof(T).Name);
#endif
        }
    }

    /// <summary>
    ///     Setup <see cref="InstancinatorConfiguration"/>.
    /// </summary>
    /// <param name="conf">The <see cref="Configuration"/>.</param>
    internal void Setup(Configuration conf)
    {
        // migration only
        this.configDirectory = conf.Pi?.ConfigDirectory;
        this.Save(true);
    }

    private static T Load(DalamudPluginInterface pi)
    {
        T? config = default;

        var file = Path.Combine(pi.ConfigDirectory.FullName, GetConfigFileName());
        if (File.Exists(file))
        {
#if DEBUG
            PluginLog.Information("Load {0}", typeof(T).Name);
#endif
            var data = File.ReadAllText(file);
            try
            {
                config = (T?)JsonSerializer.Deserialize(data, typeof(T), SerializerOptions);
            }
            catch (Exception e)
            {
                PluginLog.LogError(e, "Error during loading of configuration");
                config = new T
                {
                    ConfigError = true,
                };
            }
        }

        config ??= new T();
        config.configDirectory = pi.ConfigDirectory;
        return config;
    }

    private static string GetConfigFileName() => $"{typeof(T).Name}.json";
}