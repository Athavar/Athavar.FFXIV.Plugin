// <copyright file="Configuration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.RegularExpressions;
using System.Timers;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Config.Extensions;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Logging;
using Dalamud.Plugin;
using Lumina.Data;
using Newtonsoft.Json;

public class Configuration : IPluginConfiguration
{
    [JsonIgnore]
    private DalamudPluginInterface? pi;

    [JsonIgnore]
    private Timer? saveTimer;

    /// <inheritdoc />
    public int Version { get; set; } = 1;

    public YesConfiguration? Yes { get; set; }

    public MacroConfiguration? Macro { get; set; }

    public InstancinatorConfiguration? Instancinator { get; set; }

    public AutoSpearConfiguration? AutoSpear { get; set; }

    public CraftQueueConfiguration? CraftQueue { get; set; }

    public OpcodeWizardConfiguration? OpcodeWizard { get; set; }

    public DpsConfiguration? Dps { get; set; }

    public bool ShowToolTips { get; set; } = true;

    public Language Language { get; set; } = Language.English;

    /// <summary>
    ///     Gets or sets the chat channel to use.
    /// </summary>
    public XivChatType ChatType { get; set; } = XivChatType.Debug;

    /// <summary>
    ///     Gets or sets the error chat channel to use.
    /// </summary>
    public XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

    public static Configuration Load(DalamudPluginInterface pi)
    {
        if (pi.ConfigFile.Exists)
        {
            // upgrade format
            UpgradeConfiguration(pi.ConfigFile.FullName);
        }

        var c = (Configuration?)pi.GetPluginConfig() ?? new Configuration();
        c.Setup(pi);
        return c;
    }

    /// <summary>
    ///     Save the configuration.
    /// </summary>
    /// <param name="instant">Indicate if the configuration should be saved instant.</param>
    public void Save(bool instant = false)
    {
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
            this.pi?.SavePluginConfig(this);
#if DEBUG
            PluginLog.Information("Save Successful");
#endif
        }
        else if (!this.saveTimer.Enabled)
        {
            this.saveTimer.Start();
#if DEBUG
            PluginLog.Information("Save Triggered");
#endif
        }
    }

    public void Dispose()
    {
        this.saveTimer?.Dispose();
        this.saveTimer = null;
    }

    private static void UpgradeConfiguration(string filePath)
    {
        var config = File.ReadAllText(filePath);
        var r = new Regex("\"\\$type\": \"Athavar\\.FFXIV\\.Plugin\\.[a-zA-z.]+, (?<assembly>Athavar\\.FFXIV\\.Plugin)\"");
        var newConfig = r.ReplaceGroupValue(config, "assembly", "Athavar.FFXIV.Plugin.Config");
        if (newConfig.Length != config.Length)
        {
            File.WriteAllText(filePath, newConfig);
        }
    }

    /// <summary>
    ///     Setup <see cref="DalamudPluginInterface" />.
    /// </summary>
    /// <param name="interface">The <see cref="DalamudPluginInterface" />.</param>
    private void Setup(DalamudPluginInterface @interface)
    {
        this.pi = @interface;

        this.Yes ??= new YesConfiguration();
        this.Macro ??= new MacroConfiguration();
        this.Instancinator ??= new InstancinatorConfiguration();
        this.AutoSpear ??= new AutoSpearConfiguration();
        this.CraftQueue ??= new CraftQueueConfiguration();
        this.OpcodeWizard ??= new OpcodeWizardConfiguration();
        this.Dps ??= new DpsConfiguration();

        this.Yes.Setup(this);
        this.Macro.Setup(this);
        this.Instancinator.Setup(this);
        this.AutoSpear.Setup(this);
        this.CraftQueue.Setup(this);
        this.OpcodeWizard.Setup(this);
        this.Dps.Setup(this);
    }
}