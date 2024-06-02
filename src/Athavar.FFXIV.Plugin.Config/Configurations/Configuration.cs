// <copyright file="Configuration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Config.Extensions;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;
using Lumina.Data;
using Newtonsoft.Json;

public sealed class Configuration : IPluginConfiguration
{
    /// <inheritdoc/>
    public int Version { get; set; } = 2;

    [JsonIgnore]
    internal DalamudPluginInterface? Pi { get; private set; }

    [JsonProperty]
    private bool ShowToolTips { get; set; } = true;

    [JsonProperty]
    private bool ShowLaunchButton { get; set; }

    [JsonProperty]
    private Language Language { get; set; } = Language.English;

    /// <summary>
    ///     Gets or sets the chat channel to use.
    /// </summary>
    [JsonProperty]
    private XivChatType ChatType { get; set; } = XivChatType.Debug;

    /// <summary>
    ///     Gets or sets the error chat channel to use.
    /// </summary>
    [JsonProperty]
    private XivChatType ErrorChatType { get; set; } = XivChatType.Urgent;

    [JsonProperty]
    private YesConfiguration? Yes { get; set; }

    [JsonProperty]
    private MacroConfiguration? Macro { get; set; }

    [JsonProperty]
    private InstancinatorConfiguration? Instancinator { get; set; }

    [JsonProperty]
    private AutoSpearConfiguration? AutoSpear { get; set; }

    [JsonProperty]
    private CraftQueueConfiguration? CraftQueue { get; set; }

    [JsonProperty]
    private OpcodeWizardConfiguration? OpcodeWizard { get; set; }

    [JsonProperty]
    private DpsConfiguration? Dps { get; set; }

    [JsonProperty]
    private SliceIsRightConfiguration? SliceIsRight { get; set; }

    public static void Migrate(DalamudPluginInterface pi)
    {
        if (pi.ConfigFile.Exists)
        {
            // upgrade format
            UpgradeConfiguration(pi.ConfigFile.FullName);
        }

        var c = (Configuration?)pi.GetPluginConfig() ?? new Configuration();

        if (c.Version == 1)
        {
            c.Setup(pi);
            pi.ConfigFile.MoveTo(Path.Combine(pi.ConfigFile.Directory?.FullName ?? string.Empty, $"{pi.ConfigFile.Name}.old"), true);
        }
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
    ///     Setup <see cref="DalamudPluginInterface"/>.
    /// </summary>
    /// <param name="interface">The <see cref="DalamudPluginInterface"/>.</param>
    private void Setup(DalamudPluginInterface @interface)
    {
        // migration only
        this.Pi = @interface;

        this.Yes ??= new YesConfiguration();
        this.Macro ??= new MacroConfiguration();
        this.Instancinator ??= new InstancinatorConfiguration();
        this.AutoSpear ??= new AutoSpearConfiguration();
        this.CraftQueue ??= new CraftQueueConfiguration();
        this.OpcodeWizard ??= new OpcodeWizardConfiguration();
        this.Dps ??= new DpsConfiguration();
        this.SliceIsRight ??= new SliceIsRightConfiguration();

        var configDirectory = this.Pi.ConfigDirectory;
        this.Yes.Setup(configDirectory);
        this.Macro.Setup(configDirectory);
        this.Instancinator.Setup(configDirectory);
        this.AutoSpear.Setup(configDirectory);
        this.CraftQueue.Setup(configDirectory);
        this.OpcodeWizard.Setup(configDirectory);
        this.Dps.Setup(configDirectory);
        this.SliceIsRight.Setup(configDirectory);
#pragma warning disable 612,618
        var cconfig = new CommonConfiguration
        {
            Version = 2,
            Language = this.Language,
            ChatType = this.ChatType,
            ErrorChatType = this.ErrorChatType,
            ShowLaunchButton = this.ShowLaunchButton,
            ShowToolTips = this.ShowToolTips,
        };
        cconfig.Setup(configDirectory);
#pragma warning restore 612,618
        cconfig.Save();
    }
}